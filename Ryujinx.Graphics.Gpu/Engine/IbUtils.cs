using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    /// <summary>
    /// Index buffer utility methods.
    /// </summary>
    static class IbUtils
    {
        /// <summary>
        /// Minimum size that the vertex buffer must have, in bytes, to make the index counting profitable.
        /// </summary>
        private const ulong MinimumVbSizeThreshold = 0x200000; // 2 MB

        /// <summary>
        /// Maximum number of indices that the index buffer may have to make the index counting profitable.
        /// </summary>
        private const int MaximumIndexCountThreshold = 8192;

        /// <summary>
        /// Checks if getting the vertex buffer size from the maximum index buffer index is worth it.
        /// </summary>
        /// <param name="vbSizeMax">Maximum size that the vertex buffer may possibly have, in bytes</param>
        /// <param name="indexCount">Total number of indices on the index buffer</param>
        /// <returns>True if getting the vertex buffer size from the index buffer may yield performance improvements</returns>
        public static bool IsIbCountingProfitable(ulong vbSizeMax, int indexCount)
        {
            return vbSizeMax >= MinimumVbSizeThreshold && indexCount <= MaximumIndexCountThreshold;
        }

        /// <summary>
        /// Gets the vertex count of the vertex buffer accessed with the indices from the current index buffer.
        /// </summary>
        /// <param name="mm">GPU memory manager</param>
        /// <param name="state">Current GPU state</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        /// <returns>Vertex count</returns>
        public static ulong GetVertexCount(MemoryManager mm, GpuState state, int firstIndex, int indexCount)
        {
            var indexBuffer = state.Get<IndexBufferState>(MethodOffset.IndexBufferState);

            ulong gpuVa = indexBuffer.Address.Pack();
            uint max = 0;

            switch (indexBuffer.Type)
            {
                case IndexType.UByte:
                    {
                        ReadOnlySpan<byte> data = mm.GetSpan(gpuVa, firstIndex + indexCount);
                        for (int i = firstIndex; i < data.Length; i++)
                        {
                            if (max < data[i]) max = data[i];
                        }
                        break;
                    }
                case IndexType.UShort:
                    {
                        ReadOnlySpan<ushort> data = MemoryMarshal.Cast<byte, ushort>(mm.GetSpan(gpuVa, (firstIndex + indexCount) * 2));
                        for (int i = firstIndex; i < data.Length; i++)
                        {
                            if (max < data[i]) max = data[i];
                        }
                        break;
                    }
                case IndexType.UInt:
                    {
                        ReadOnlySpan<uint> data = MemoryMarshal.Cast<byte, uint>(mm.GetSpan(gpuVa, (firstIndex + indexCount) * 4));
                        for (int i = firstIndex; i < data.Length; i++)
                        {
                            if (max < data[i]) max = data[i];
                        }
                        break;
                    }
            }

            return (ulong)max + 1;
        }
    }
}
