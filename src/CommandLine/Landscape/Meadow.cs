using System;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Meadow
    {
        public static void Draw(
            WorldBuilder world,
            Block stem,
            Block[] flower,
            Point3 position,
            int flowerCount,
            int radius,
            int seed,
            bool hasFlowers = true)
        {
            var random = new Random(seed);
            for(int i = 0; i < flowerCount; ++i)
            {
                var angle = 2 * MathF.PI * random.Next(0, 360) / 360f;
                var offset = random.Next(0, radius);
                var current = Point3.Round(new Vector3(MathF.Cos(angle) * offset, position.Y, MathF.Sin(angle) * offset));
                var height = random.Next(1, 4);
                world.FillLine(
                    current + new Point3(0, -2, 0),
                    current + new Point3(0, height, 0),
                    stem);
                if (hasFlowers)
                {
                    world[current + new Point3(0, height + 1, 0)] = flower[random.Next(0, flower.Length)];
                }
            }
        }
    }
}
