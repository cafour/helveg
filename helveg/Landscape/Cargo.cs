using System;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Cargo
    {
        public const int Side = 11;
        public const int ContainerSize = 3;

        public static void Draw(
            WorldBuilder world,
            Point3 position,
            Block platform,
            Block corner,
            Block[] containers,
            int seed,
            int containerCount)
        {
            var outer = new Square(position, Side - 1);
            world.FillVolume(outer.LowerLeft, outer.UpperRight, platform);
            world.FillVolume(outer.LowerLeft, new Point3(outer.LowerLeft.X - 1, 0, outer.LowerLeft.Z - 1), corner);
            world.FillVolume(outer.LowerRight, new Point3(outer.LowerRight.X + 1, 0, outer.LowerRight.Z - 1), corner);
            world.FillVolume(outer.UpperLeft, new Point3(outer.UpperLeft.X - 1, 0, outer.UpperLeft.Z + 1), corner);
            world.FillVolume(outer.UpperRight, new Point3(outer.UpperRight.X + 1, 0, outer.UpperRight.Z + 1), corner);

            var inner = new Square(position, Side - 3);
            var random = new Random(seed);
            var orientation = random.Next() % 2;
            var increments = new []
            {
                (from: new Point3(1, 0, 0), to: new Point3(ContainerSize, ContainerSize - 1, Side - 3)),
                (from: new Point3(0, 0, 1), to: new Point3(Side - 3, ContainerSize - 1, ContainerSize))
            };
            for (int i = 0; i < containerCount / 2; ++i)
            {
                var (from, to) = increments[(orientation + i) % 2];
                var height =  new Point3(0, 1 + ContainerSize * i, 0);
                var invert = new Point3(-1, 1, -1);
                world.FillVolume(
                    one: inner.LowerLeft + from + height,
                    two: inner.LowerLeft + to + height,
                    fill: containers[random.Next() % containers.Length]);
                world.FillVolume(
                    one: inner.UpperRight + from * invert + height,
                    two: inner.UpperRight + to * invert + height,
                    fill: containers[random.Next() % containers.Length]);
            }

            if (containerCount % 2 == 1)
            {
                var (from, to) = increments[(orientation + containerCount / 2) % 2];
                var height = 1 + containerCount / 2 * ContainerSize;
                world.FillVolume(
                    one: inner.LowerLeft + from + new Point3(0, height, 0),
                    two: inner.LowerLeft + to + new Point3(0, height, 0),
                    fill: containers[random.Next() % containers.Length]);
            }

        }
    }
}
