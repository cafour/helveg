using System;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Signpost
    {
        public const int BaseHeight = 4;
        public const int ArrowLength = 2;
        public const int ArrowWidth = 1;
        public const int AngleCount = 8;

        public static void Draw(
            WorldBuilder world,
            Block wood,
            Block[] arrows,
            Point3 position,
            int arrowCount,
            int seed)
        {
            var random = new Random(seed);
            for (int i = 0; i < arrowCount; ++i)
            {
                var angle =  MathF.PI * 2.0f / AngleCount * random.Next(0, AngleCount);
                var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                var end = new Vector3(direction.X, 0, direction.Y) * ArrowLength;
                for (int y = BaseHeight + i; y < BaseHeight + i + ArrowWidth; ++y)
                {
                    world.FillLine(
                        from: position + new Point3(0, y, 0),
                        to: position + Point3.Round(end) + new Point3(0, y, 0),
                        fill: arrows[random.Next() % arrows.Length],
                        corners: true);
                }
                
            }
            world.FillLine(position, position + new Point3(0, BaseHeight + arrowCount, 0), wood);
        }
    }
}
