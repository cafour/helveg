using System;
using System.Numerics;

namespace Helveg
{
    public struct Chunk
    {
        public Chunk(Vector3[,,] voxels)
        {
            if (voxels.GetLength(0) != voxels.GetLength(1)
                || voxels.GetLength(1) != voxels.GetLength(2))
            {
                throw new ArgumentException("The colors array must be a cube.");
            }
            Voxels = voxels;
        }

        public Vector3[,,] Voxels { get; }

        public unsafe struct Raw
        {
            public Vector3* Voxels;
            public int Size;
        }
    }
}
