using System.Numerics;

namespace Helveg.Landscape
{
    public static class Colours
    {
        public static readonly Vector3[] IslandPalette = new Vector3[]
        {
            new Vector3(146, 146, 146) / 255, // stone
            new Vector3(109, 182, 73) / 255, // grass
            new Vector3(219, 182, 146) / 255, // sand
            new Vector3(109, 73, 36) / 255, // wood
            new Vector3(0, 146, 219) / 255, // water
            new Vector3(0, 42, 0) / 255 // leaves
        };
    }
}
