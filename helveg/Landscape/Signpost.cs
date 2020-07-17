using System;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Signpost
    {
        public const int BaseHeight = 4;
        public const int ArrowLength = 3;
        public const int ArrowWidth = 2;
        public const int AngleCount = 8;

        public static void Draw(
            WorldBuilder world,
            Block wood,
            Block[] arrows,
            Point3 position,
            int arrowCount,
            int seed)
        {
            world.FillVolume(position, position + new Point3(1, BaseHeight + arrowCount, 1), wood);
            var random = new Random(seed);
            for (int i = 0; i < arrowCount; ++i)
            {
                var angle =  MathF.PI * 2.0f / AngleCount * random.Next(0, AngleCount);
                var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                var cornerX = angle < MathF.PI / 2 || angle > 3 * MathF.PI / 2 ? 1 : 0;
                var cornerY = angle < Math.PI ? 1 : 0;
                var corner = position + new Point3(cornerX, 0, cornerY);
                var end = new Vector3(direction.X, 0, direction.Y) * ArrowLength;
                for (int y = BaseHeight + i; y < BaseHeight + i + ArrowWidth; ++y)
                {
                    world.FillLine(
                        from: corner + new Point3(0, y, 0),
                        to: corner + Point3.Round(end) + new Point3(0, y, 0),
                        fill: arrows[random.Next() % arrows.Length],
                        corners: true);
                }
                
            }
        }
    }
}
