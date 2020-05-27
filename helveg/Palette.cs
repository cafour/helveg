using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg
{
    public struct Palette
    {
        public const int PalleteSize = 256;

        public Palette(Vector3[] colors)
        {
            if (colors.Length != 256)
            {
                throw new ArgumentException("Every pallete must have 256 elements.");
            }

            Colors = colors;
        }

        public Vector3[] Colors { get; }
    }
}
