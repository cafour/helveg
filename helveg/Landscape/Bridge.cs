using System;
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
            Block[] bridge,
            int height)
        {
            var direction = Vector3.Normalize((Vector3)(to - from));
            var normal = Point3.Round(Width * Vector3.Cross(direction, new Vector3(0, 1, 0)));
            var dir = Point3.Round(Width * direction);
            var left = normal - dir;
            var right = -normal - dir;
            var length = (to - from).Length();

            void drawSide(Point3 s)
            {
                world[s] = bridge[0];
                var offset = s - from;
                world.OverLine(s, to + offset, p =>
                {
                    var distance = (p - s).Length();
                    var percentage = distance / length;
                    var currentHeight = (MathF.Sin(3 * MathF.PI / 2 + 2 * MathF.PI * percentage) + 1.0f) * 0.5f * height;
                    var currentLevel = (int)MathF.Round(currentHeight);
                    p.Y += (int)MathF.Round(currentHeight);
                    var index = currentLevel % bridge.Length;
                    world[p] = bridge[index];
                }, true);
            }

            world.OverLine(from, from + left, drawSide);
            world.OverLine(from, from + right, drawSide);
        }
    }
}
