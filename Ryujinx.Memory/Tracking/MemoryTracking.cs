﻿using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory.Tracking
{
    public class MemoryTracking
    {
        private NonOverlappingRangeList<VirtualRegion> _virtualRegions;
        private NonOverlappingRangeList<PhysicalRegion> _physicalRegions;

        private IVirtualMemoryManager _memoryManager;

        private MemoryBlock _block;
        internal object TrackingLock = new object();

        // Only use these from within the lock.
        private VirtualRegion[] _virtualResults = new VirtualRegion[10];
        private PhysicalRegion[] _physicalResults = new PhysicalRegion[10];

        private int _pageSize;

        public MemoryTracking(IVirtualMemoryManager memoryManager, MemoryBlock block, int pageSize)
        {
            _memoryManager = memoryManager;
            _block = block;
            _pageSize = pageSize;

            _virtualRegions = new NonOverlappingRangeList<VirtualRegion>();
            _physicalRegions = new NonOverlappingRangeList<PhysicalRegion>();

            _block.RegisterTrackingAction(PhysicalMemoryEvent);
        }

        public void Map(ulong va, ulong pa, ulong size)
        {
            // A mapping may mean we need to re-evaluate each VirtualRegion's affected area.
            // Find all handles that overlap with the range, we need to recalculate their physical regions

            lock (TrackingLock)
            {
                var results = _virtualResults;
                int count = _virtualRegions.FindOverlapsNonOverlapping(va, size, ref results);

                for (int i = 0; i < count; i++)
                {
                    VirtualRegion region = results[i];
                    region.RecalculatePhysicalChildren();
                }
            }
        }

        public void Unmap(ulong va, ulong size)
        {
            // An unmapping may mean we need to re-evaluate each VirtualRegion's affected area.
            // Find all handles that overlap with the range, we need to recalculate their physical regions

            lock (TrackingLock)
            {
                var results = _virtualResults;
                int count = _virtualRegions.FindOverlapsNonOverlapping(va, size, ref results);

                for (int i = 0; i < count; i++)
                {
                    VirtualRegion region = results[i];
                    region.RecalculatePhysicalChildren();
                }
            }
        }

        internal List<VirtualRegion> GetVirtualRegionsForHandle(ulong va, ulong size)
        {
            List<VirtualRegion> result = new List<VirtualRegion>();
            _virtualRegions.GetOrAddRegions(result, va, size, (va, size) => new VirtualRegion(this, va, size));

            return result;
        }

        internal List<PhysicalRegion> GetPhysicalRegionsForVirtual(ulong va, ulong size)
        {
            List<PhysicalRegion> result = new List<PhysicalRegion>();

            // Get a list of physical regions for this virtual region, from our injected virtual mapping function.
            (ulong Address, ulong Size)[] physicalRegions = _memoryManager.GetPhysicalRegions(va, size);

            foreach (var region in physicalRegions)
            {
                _physicalRegions.GetOrAddRegions(result, region.Address, region.Size, (pa, size) => new PhysicalRegion(this, pa, size));
            }

            return result;
        }

        internal void RemoveVirtual(VirtualRegion region)
        {
            _virtualRegions.Remove(region);
        }

        internal void RemovePhysical(PhysicalRegion region)
        {
            _physicalRegions.Remove(region);
        }

        private (ulong address, ulong size) PageAlign(ulong address, ulong size)
        {
            ulong pageMask = (ulong)_pageSize - 1;
            ulong rA = address & (~pageMask);
            ulong rS = ((address + size + pageMask) & (~pageMask)) - rA;
            return (rA, rS);
        }

        public MultiRegionHandle BeginGranularTracking(ulong address, ulong size, ulong granularity)
        {
            (address, size) = PageAlign(address, size);

            return new MultiRegionHandle(this, address, size, granularity);
        }

        public RegionHandle BeginTracking(ulong address, ulong size)
        {
            (address, size) = PageAlign(address, size);

            lock (TrackingLock)
            {
                RegionHandle handle = new RegionHandle(this, address, size);

                return handle;
            }
        }

        public bool PhysicalMemoryEvent(ulong address, bool write)
        {
            // Look up the physical region using the region list.
            // Signal up the chain to relevant handles.

            lock (TrackingLock)
            {
                var results = _physicalResults;
                int count = _physicalRegions.FindOverlapsNonOverlapping(address, 8, ref results); // TODO: get/use the actual access size?

                if (count == 0)
                {
                    _block.ReprotectMirror(address & ~(ulong)(_pageSize - 1), 4096, MemoryPermission.ReadAndWrite);
                    return true; // We can't handle this - unprotect and return.
                }

                for (int i = 0; i < count; i++)
                {
                    PhysicalRegion region = results[i];
                    region.Signal(write);
                }
            }

            return true;
        }

        public bool VirtualMemoryEvent(ulong address, ulong size, bool write)
        {
            // Look up the virtual region using the region list.
            // Signal up the chain to relevant handles.

            lock (TrackingLock)
            {
                var results = _virtualResults;
                int count = _virtualRegions.FindOverlapsNonOverlapping(address, size, ref results);

                if (count == 0)
                {
                    return false; // We can't handle this - it's probably a real invalid access.
                }

                for (int i = 0; i < count; i++)
                {
                    VirtualRegion region = results[i];
                    region.Signal(write);
                }
            }

            return true;
        }

        internal void ProtectPhysicalRegion(PhysicalRegion region, MemoryPermission permission)
        {
            _block.ReprotectMirror(region.Address, region.Size, permission);
        }

        internal void ProtectVirtualRegion(VirtualRegion region, MemoryPermission permission)
        {
            _memoryManager.Reprotect(region.Address, region.Size, permission);
        }
    }
}
