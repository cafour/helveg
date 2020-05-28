using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg
{
    public struct Chunk
    {
        private GCHandle voxelsHandle;
        private GCHandle paletteHandle;

        public Chunk(Block[,,] voxels, Vector3[] palette)
        {
            if (voxels.GetLength(0) != voxels.GetLength(1)
                || voxels.GetLength(1) != voxels.GetLength(2))
            {
                throw new ArgumentException("The colors array must be a cube.");
            }
            Voxels = voxels;
            voxelsHandle = default;
            Palette = palette;
            paletteHandle = default;
        }

        public Block[,,] Voxels { get; }
        public Vector3[] Palette { get; }

        public unsafe Raw GetRaw()
        {
            if (!voxelsHandle.IsAllocated)
            {
                voxelsHandle = GCHandle.Alloc(Voxels, GCHandleType.Pinned);
            }
            
            if (!paletteHandle.IsAllocated)
            {
                paletteHandle = GCHandle.Alloc(Palette, GCHandleType.Pinned);
            }

            return new Raw
            {
                Voxels = (Block *)voxelsHandle.AddrOfPinnedObject().ToPointer(),
                Palette = (Vector3 *)paletteHandle.AddrOfPinnedObject().ToPointer(),
                Size = Voxels.GetLength(0)
            };
        }

        public unsafe struct Raw
        {
            public Block* Voxels;
            public Vector3 *Palette;
            public int Size;
        }
    }
}
