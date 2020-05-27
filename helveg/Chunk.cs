using System;
using System.Numerics;

namespace Helveg
{
    public struct Chunk
    {
        public Chunk(Block[,,] voxels, Palette palette)
        {
            if (voxels.GetLength(0) != voxels.GetLength(1)
                || voxels.GetLength(1) != voxels.GetLength(2))
            {
                throw new ArgumentException("The colors array must be a cube.");
            }
            Voxels = voxels;
            Palette = palette;
        }

        public Block[,,] Voxels { get; }
        public Palette Palette { get; }

        public unsafe struct Raw
        {
            public Block* Voxels;
            public Vector3 *Palette;
            public int Size;
        }
    }
}
