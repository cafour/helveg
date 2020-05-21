using System;
using System.Numerics;

namespace Helveg
{
    public struct Chunk
    {
        public Chunk(Vector3[,,] colors)
        {
            if (colors.GetLength(0) != colors.GetLength(1)
                || colors.GetLength(1) != colors.GetLength(2))
            {
                throw new ArgumentException("The colors array must be a cube.");
            }
            Colors = colors;
        }

        public Vector3[,,] Colors { get; }

        public unsafe struct Raw
        {
            public Vector3* Colors;
            public int Side;
        }
    }
}
