using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.RomFs;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.Loaders.Mods;
using Ryujinx.HLE.Loaders.Executables;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;

namespace Ryujinx.HLE.HOS
{
    public class ModLoader
    {
        private readonly VirtualFileSystem _vfs;

        private const string RootModsDir = "mods";
        private const string RomfsDir = "romfs";
        private const string RomfsStorageFile = "romfs.storage";
        private const string ExefsDir = "exefs";
        private const string NsoPatchesDir = "exefs_patches";
        private const string NroPatchesDir = "nro_patches";
        private const string StubExtension = ".stub";

        public class ModEntry
        {
            public readonly ulong TitleId;
            public readonly DirectoryInfo ModDir;
            public readonly DirectoryInfo Exefs;
            public readonly DirectoryInfo Romfs;
            public readonly FileInfo RomfsFile;

            private bool _enabled;

            public ModEntry(DirectoryInfo modDir, bool enabled, ulong titleId = ulong.MaxValue)
            {
                ModDir = modDir;
                _enabled = enabled;

                if (titleId == ulong.MaxValue) // Global mod
                {
                    Exefs = ModDir;
                }
                else // Title mod
                {
                    Exefs = new DirectoryInfo(Path.Combine(modDir.FullName, ExefsDir));
                    Romfs = new DirectoryInfo(Path.Combine(modDir.FullName, RomfsDir));
                    RomfsFile = new FileInfo(Path.Combine(modDir.FullName, RomfsStorageFile));
                }

                if (Empty)
                {
                    Logger.PrintWarning(LogClass.Application, $"{ModName} is empty");
                }
            }

            // Useful when Init and processing are separated
            public bool Recheck()
            {
                if (!_enabled)
                {
                    return false;
                }

                ModDir.Refresh();
                Exefs.Refresh();
                Romfs?.Refresh();
                RomfsFile?.Refresh();
                return ModDir.Exists;
            }

            public string ModName => ModDir.Name;
            public bool Empty => !(Exefs.Exists || RomfsFile.Exists || Romfs.Exists);

            public override string ToString() => $"[{(Exefs.Exists ? "E" : "")}{(RomfsFile.Exists ? "r" : "")}{(Romfs.Exists ? "R" : "")}] '{ModName}'";
        }

        public List<string> ModRootDirs { get; private set; }

        public Dictionary<ulong, List<ModEntry>> TitleMods { get; private set; }
        public List<ModEntry> GlobalNsoMods { get; private set; } // NsoPatchesDir
        public List<ModEntry> GlobalNroMods { get; private set; } // NroPatchesDir

        public ModLoader(VirtualFileSystem vfs)
        {
            _vfs = vfs;

            // By default, mods are collected from RyujinxBasePath/{RootModsDir}
            ModRootDirs = new List<string> { Path.Combine(_vfs.GetBasePath(), RootModsDir) };
        }

        public void InitModsList()
        {
            TitleMods = new Dictionary<ulong, List<ModEntry>>();
            GlobalNsoMods = new List<ModEntry>();
            GlobalNroMods = new List<ModEntry>();

            // Duplicate mod name checking
            var modNames = new HashSet<string>();
            void ModNameCheck(DirectoryInfo modDir)
            {
                if (!modNames.Add(modDir.Name))
                {
                    Logger.PrintWarning(LogClass.Application, $"Duplicate mod name '{modDir.Name}'");
                }
            }

            foreach (string modRootPath in ModRootDirs)
            {
                var modRootDir = new DirectoryInfo(modRootPath);
                if (!modRootDir.Exists) continue;

                Logger.PrintDebug(LogClass.Application, $"Loading mods from `{modRootPath}`");

                foreach (var titleDir in modRootDir.EnumerateDirectories())
                {
                    switch (titleDir.Name)
                    {
                        case NsoPatchesDir:
                        case NroPatchesDir:
                            foreach (var modDir in titleDir.EnumerateDirectories())
                            {
                                var modEntry = new ModEntry(modDir, true);

                                (titleDir.Name == NsoPatchesDir ? GlobalNsoMods : GlobalNroMods).Add(modEntry);
                                Logger.PrintInfo(LogClass.Application, $"Found exefs_patches Mod '{modDir.Name}'");

                                ModNameCheck(modDir);
                            }
                            break;

                        default:
                            if (titleDir.Name.Length >= 16 && ulong.TryParse(titleDir.Name.Substring(0, 16), System.Globalization.NumberStyles.HexNumber, null, out ulong titleId))
                            {
                                foreach (var modDir in titleDir.EnumerateDirectories())
                                {
                                    var modEntry = new ModEntry(modDir, true, titleId);

                                    Logger.PrintInfo(LogClass.Application, $"Found Mod [{titleId:X16}] {modEntry}");

                                    ModNameCheck(modDir);

                                    if (TitleMods.TryGetValue(titleId, out List<ModEntry> modEntries))
                                    {
                                        modEntries.Add(modEntry);
                                    }
                                    else
                                    {
                                        TitleMods.Add(titleId, new List<ModEntry> { modEntry });
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        internal IStorage ApplyRomFsMods(ulong titleId, IStorage baseStorage)
        {
            if (TitleMods.TryGetValue(titleId, out var titleMods))
            {
                var enabledMods = titleMods.Where(mod => mod.Recheck());

                var romfsContainers = enabledMods
                                      .Where(mod => mod.RomfsFile.Exists)
                                      .Select(mod => mod.RomfsFile);

                var romfsDirs = enabledMods
                                .Where(mod => !mod.RomfsFile.Exists && mod.Romfs.Exists)
                                .Select(mod => mod.Romfs);

                var fileSet = new HashSet<string>();
                var builder = new RomFsBuilder();

                int appliedCount = 0;

                Logger.PrintInfo(LogClass.Loader, "Collecting RomFs Containers...");
                appliedCount += CollectRomFsMods(romfsContainers, fileSet, builder);

                Logger.PrintInfo(LogClass.Loader, "Collecting RomFs Dirs...");
                appliedCount += CollectRomFsMods(romfsDirs, fileSet, builder);

                if (appliedCount == 0)
                {
                    Logger.PrintInfo(LogClass.Loader, "Using base RomFs");
                    return baseStorage;
                }

                Logger.PrintInfo(LogClass.Loader, $"Found {fileSet.Count} modded files over {appliedCount} mods. Processing base storage...");
                var baseRfs = new RomFsFileSystem(baseStorage);

                foreach (var entry in baseRfs.EnumerateEntries()
                                             .Where(f => f.Type == DirectoryEntryType.File && !fileSet.Contains(f.FullPath))
                                             .OrderBy(f => f.FullPath, StringComparer.Ordinal))
                {
                    baseRfs.OpenFile(out IFile file, entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                    builder.AddFile(entry.FullPath, file);
                }

                Logger.PrintInfo(LogClass.Loader, "Building new RomFs...");
                IStorage newStorage = builder.Build();
                Logger.PrintInfo(LogClass.Loader, "Using modded RomFs");

                return newStorage;
            }

            return baseStorage;
        }

        private int CollectRomFsMods(IEnumerable<FileSystemInfo> fsEntries, HashSet<string> fileSet, RomFsBuilder builder)
        {
            static IFileSystem OpenFS(FileSystemInfo e) => e switch
            {
                DirectoryInfo romfsDir => new LocalFileSystem(romfsDir.FullName),
                FileInfo romfsContainer => new RomFsFileSystem(romfsContainer.OpenRead().AsStorage()),
                _ => null
            };

            static string GetModName(FileSystemInfo e) => e switch
            {
                DirectoryInfo romfsDir => romfsDir.Parent.Name,
                FileInfo romfsContainer => romfsContainer.Directory.Name,
                _ => null
            };

            int modCount = 0;

            foreach (var fsEntry in fsEntries)
            {
                using IFileSystem fs = OpenFS(fsEntry);
                foreach (var entry in fs.EnumerateEntries()
                                       .Where(f => f.Type == DirectoryEntryType.File)
                                       .OrderBy(f => f.FullPath, StringComparer.Ordinal))
                {
                    fs.OpenFile(out IFile file, entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                    if (fileSet.Add(entry.FullPath))
                    {
                        builder.AddFile(entry.FullPath, file);
                    }
                    else
                    {
                        Logger.PrintWarning(LogClass.Loader, $"    Skipped duplicate file '{entry.FullPath}' from '{GetModName(fsEntry)}'");
                    }
                }

                modCount++;
            }

            return modCount;
        }

        internal void ApplyExefsReplacements(ulong titleId, List<NsoExecutable> nsos)
        {
            if (nsos.Count > 32)
            {
                throw new ArgumentOutOfRangeException("NSO Count is more than 32");
            }

            var exefsDirs = (TitleMods.TryGetValue(titleId, out var titleMods) ? titleMods : Enumerable.Empty<ModEntry>())
                            .Where(mod => mod.Recheck() && mod.Exefs.Exists)
                            .Select(mod => mod.Exefs);

            BitVector32 stubs = new BitVector32();
            BitVector32 repls = new BitVector32();

            foreach (var exefsDir in exefsDirs)
            {
                for (int i = 0; i < nsos.Count; ++i)
                {
                    var nso = nsos[i];
                    var nsoName = nso.Name;

                    FileInfo nsoFile = new FileInfo(Path.Combine(exefsDir.FullName, nsoName));
                    if (nsoFile.Exists)
                    {
                        if (repls[1 << i])
                        {
                            Logger.PrintWarning(LogClass.Loader, $"Multiple replacements to '{nsoName}'");
                            continue;
                        }

                        repls[1 << i] = true;

                        nsos[i] = new NsoExecutable(nsoFile.OpenRead().AsStorage(), nsoName);
                        Logger.PrintInfo(LogClass.Loader, $"NSO '{nsoName}' replaced");

                        continue;
                    }

                    stubs[1 << i] |= File.Exists(Path.Combine(exefsDir.FullName, nsoName + StubExtension));
                }
            }

            for (int i = nsos.Count - 1; i >= 0; --i)
            {
                if (stubs[1 << i] && !repls[1 << i]) // Prioritizes replacements over stubs
                {
                    Logger.PrintInfo(LogClass.Loader, $"NSO '{nsos[i].Name}' stubbed");
                    nsos.RemoveAt(i);
                }
            }
        }

        internal void ApplyNroPatches(NroExecutable nro)
        {
            var nroPatches = GlobalNroMods.Where(mod => mod.Recheck() && mod.Exefs.Exists)
                             .Select(mod => mod.Exefs);

            // NRO patches aren't offset relative to header unlike NSO
            // according to Atmosphere's ro patcher module
            ApplyProgramPatches(nroPatches, 0, nro);
        }

        internal void ApplyNsoPatches(ulong titleId, params IExecutable[] programs)
        {
            var exefsDirs = (TitleMods.TryGetValue(titleId, out var titleMods) ? titleMods : Enumerable.Empty<ModEntry>())
                            .Concat(GlobalNsoMods)
                            .Where(mod => mod.Recheck() && mod.Exefs.Exists)
                            .Select(mod => mod.Exefs);

            // NSO patches are created with offset 0 according to Atmosphere's patcher module
            // But `Program` doesn't contain the header which is 0x100 bytes. So, we adjust for that here
            ApplyProgramPatches(exefsDirs, 0x100, programs);
        }

        private void ApplyProgramPatches(IEnumerable<DirectoryInfo> dirs, int protectedOffset, params IExecutable[] programs)
        {
            MemPatch[] patches = new MemPatch[programs.Length];

            for (int i = 0; i < patches.Length; ++i)
            {
                patches[i] = new MemPatch();
            }

            var buildIds = programs.Select(p => p switch
            {
                NsoExecutable nso => BitConverter.ToString(nso.BuildId).Replace("-", "").TrimEnd('0'),
                NroExecutable nro => BitConverter.ToString(nro.Header.BuildId).Replace("-", "").TrimEnd('0'),
                _ => string.Empty
            }).ToList();

            static string GetModName(DirectoryInfo exefsDir) => exefsDir.Name == ExefsDir ? exefsDir.Parent.Name : exefsDir.Name;

            int GetIndex(string buildId) => buildIds.FindIndex(id => id == buildId); // O(n) but list is small

            // Collect patches
            foreach (var patchDir in dirs)
            {
                foreach (var patchFile in patchDir.EnumerateFiles())
                {
                    switch (patchFile.Extension)
                    {
                        case ".ips":
                            {
                                string filename = Path.GetFileNameWithoutExtension(patchFile.FullName).Split('.')[0];
                                string buildId = filename.TrimEnd('0');

                                int index = GetIndex(buildId);
                                if (index == -1)
                                {
                                    continue;
                                }

                                Logger.PrintInfo(LogClass.Loader, $"Found IPS patch '{GetModName(patchDir)}'/'{patchFile.Name}' bid={buildId}");

                                using var fs = patchFile.OpenRead();
                                using var reader = new BinaryReader(fs);

                                var patcher = new IpsPatcher(reader);
                                patcher.AddPatches(patches[index]);
                            }
                            break;

                        case ".pchtxt":
                            using (var fs = patchFile.OpenRead())
                            using (var reader = new StreamReader(fs))
                            {
                                var patcher = new IPSwitchPatcher(reader);

                                int index = GetIndex(patcher.BuildId);
                                if (index == -1)
                                {
                                    continue;
                                }

                                Logger.PrintInfo(LogClass.Loader, $"Found IPSwitch patch '{GetModName(patchDir)}'/'{patchFile.Name}' bid={patcher.BuildId}");

                                patcher.AddPatches(patches[index]);
                            }
                            break;
                    }
                }
            }

            // Apply patches
            for (int i = 0; i < programs.Length; ++i)
            {
                patches[i].Patch(programs[i].Program, protectedOffset);
            }
        }
    }
}