using System.Numerics;

namespace Helveg.Landscape
{
    public static class Colors
    {
        public const byte NeedleColourCount = 6;

        public static readonly Vector3 SkyColour = new Vector3(0.533f, 0.808f, 0.925f);

        public enum Island : byte
        {
            Stone,
            Grass,
            Sand,
            Wood,
            Water,
            Leaves,
            Needles0,
            Needles1,
            Needles2,
            Needles3,
            Needles4,
            Needles5,
            Wood0,
            Wood1,
            Roof,
            Cargo0,
            Cargo1,
            Cargo2,
            Stem,
            Flower0,
            Flower1,
            Flower2
        }

        public static readonly Vector3[] IslandPalette = new Vector3[]
        {
            new Vector3(146, 146, 146) / 255, // stone
            new Vector3(109, 182, 73) / 255, // grass
            new Vector3(219, 182, 146) / 255, // sand
            new Vector3(109, 73, 36) / 255, // wood
            new Vector3(0x00, 0x5b, 0xa8) / 0xff, // water
            new Vector3(0, 42, 0) / 255, // leaves
            new Vector3(0, 73, 36) / 255, // needles0
            new Vector3(109, 146, 109) / 255, // needles1
            new Vector3(54, 141, 115) / 255, // needles2
            new Vector3(0, 109, 0) / 255, // needles3
            new Vector3(146, 182, 146) / 255, // needles4
            new Vector3(166, 153, 51) / 255, // needles5
            new Vector3(0x95, 0x42, 0x01) / 0xff, // wood0
            new Vector3(0x58, 0x28, 0x03) / 0xff, // wood1
            new Vector3(0xb9, 0x83, 0x5d) / 0xff, // roof
            new Vector3(0xcf, 0x2c, 0x23) / 0xff, // cargo0
            new Vector3(0xde, 0xad, 0x41) / 0xff, // cargo1
            new Vector3(0x2a, 0x77, 0x58) / 0xff, // cargo2
            new Vector3(0x23, 0x38, 0x0a) / 0xff, // stem
            new Vector3(0x6a, 0x3c, 0x61) / 0xff, // flower0
            new Vector3(0x99, 0x02, 0x0e) / 0xff, // flower1
            new Vector3(0xd1, 0x9e, 0x01) / 0xff, // flower2
        };
    }
}
