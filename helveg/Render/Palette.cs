using System.Numerics;

namespace Helveg.Render
{
    public struct Palette
    {
        public const int PalleteSize = 256;

        public Palette(Vector3[] colors)
        {
            Colors = colors;
        }

        public Vector3[] Colors { get; }
    }
}
