using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Bridge
    {
        public const int SegmentLength = 6;
        public const int Width = 3;

        public static void Draw(
            WorldBuilder world,
            Point3 from,
            Point3 to,
            Block[] bridge)
        {
            var direction = Vector3.Normalize((Vector3)(to - from));
            var normal = Point3.Round(Width * Vector3.Cross(direction, new Vector3(0, 1, 0)));
            var dir = Point3.Round(Width * direction);
            var left = normal - dir;
            var right = -normal - dir;
            world.OverLine(from, to, p =>
            {
                var index = (int)((p - from).Length() / SegmentLength) % 2;
                world.FillLine(p, p + left, bridge[index], true);
                world.FillLine(p, p + right, bridge[index], true);
            }, corners: true);
        }
    }
}
