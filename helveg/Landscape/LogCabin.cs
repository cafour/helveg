using Helveg.Render;

namespace Helveg.Landscape
{
    public static class LogCabin
    {
        public static void Draw(
            WorldBuilder world,
            Point3 position,
            Block wood0,
            Block wood1,
            Block roof,
            int side,
            int levelCount,
            bool hasRoof = true)
        {
            // TODO: Clean and refactor
            var square = new Square(position, side);

            for (int i = 0; i < levelCount; ++i)
            {
                if (i % 2 == 0)
                {
                    world.FillLine(
                        square.LowerLeft + new Point3(0, i, -1),
                        square.UpperLeft + new Point3(0, i, 1),
                        wood0);
                    world.FillLine(
                        square.LowerRight + new Point3(0, i, -1),
                        square.UpperRight + new Point3(0, i, 1),
                        wood0);
                    world.FillLine(
                        square.LowerLeft + new Point3(1, i, 0),
                        square.LowerRight + new Point3(-1, i, 0),
                        wood1);
                    world.FillLine(
                        square.UpperLeft + new Point3(1, i, 0),
                        square.UpperRight + new Point3(-1, i, 0),
                        wood1);
                }
                else
                {
                    world.FillLine(
                        square.LowerLeft + new Point3(0, i, 1),
                        square.UpperLeft + new Point3(0, i, -1),
                        wood1);
                    world.FillLine(
                        square.LowerRight + new Point3(0, i, 1),
                        square.UpperRight + new Point3(0, i, -1),
                        wood1);
                    world.FillLine(
                        square.LowerLeft + new Point3(-1, i, 0),
                        square.LowerRight + new Point3(1, i, 0),
                        wood0);
                    world.FillLine(
                        square.UpperLeft + new Point3(-1, i, 0),
                        square.UpperRight + new Point3(1, i, 0),
                        wood0);
                }
            }
            if (levelCount % 2 == 0)
            {
                for (int i = -1; i < side / 2 + 1; ++i)
                {
                    if (i >= 0)
                    {
                        world.FillLine(
                            square.LowerLeft + new Point3(0, levelCount + i, i - 1),
                            square.UpperLeft + new Point3(0, levelCount + i, -i + 1),
                            wood0);
                        world.FillLine(
                            square.LowerRight + new Point3(0, levelCount + i, i - 1),
                            square.UpperRight + new Point3(0, levelCount + i, -i + 1),
                            wood0);
                    }
                    if (hasRoof)
                    {
                        world.FillLine(
                            square.LowerLeft + new Point3(-1, levelCount + i, i),
                            square.LowerRight + new Point3(1, levelCount + i, i),
                            roof);
                        world.FillLine(
                            square.UpperLeft + new Point3(-1, levelCount + i, -i),
                            square.UpperRight + new Point3(1, levelCount + i, -i),
                            roof);
                    }
                }
            }
            else
            {
                for (int i = -1; i < side / 2 + 1; ++i)
                {
                    if (i >= 0)
                    {
                        world.FillLine(
                            square.LowerLeft + new Point3(i - 1, levelCount + i, 0),
                            square.LowerRight + new Point3(-i + 1, levelCount + i, 0),
                            wood0);
                        world.FillLine(
                            square.UpperLeft + new Point3(i - 1, levelCount + i, 0),
                            square.UpperRight + new Point3(-i + 1, levelCount + i, 0),
                            wood0);
                    }
                    if (hasRoof)
                    {
                        world.FillLine(
                            square.LowerLeft + new Point3(i, levelCount + i, -1),
                            square.UpperLeft + new Point3(i, levelCount + i, 1),
                            roof);
                        world.FillLine(
                            square.LowerRight + new Point3(-i, levelCount + i, -1),
                            square.UpperRight + new Point3(-i, levelCount + i, 1),
                            roof);
                    }
                }
            }
        }
    }
}
