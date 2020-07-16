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
            var lowerLeft = position + new Point3(-side / 2, 0, -side / 2);
            var lowerRight = position + new Point3(side / 2 + side % 2, 0, -side / 2);
            var upperLeft = position + new Point3(-side / 2, 0, side / 2 + side % 2);
            var upperRight = position + new Point3(side / 2 + side % 2, 0, side / 2 + side % 2);

            for (int i = 0; i < levelCount; ++i)
            {
                if (i % 2 == 0)
                {
                    world.FillLine(lowerLeft + new Point3(0, i, -1), upperLeft + new Point3(0, i, 1), wood0);
                    world.FillLine(lowerRight + new Point3(0, i, -1), upperRight + new Point3(0, i, 1), wood0);
                    world.FillLine(lowerLeft + new Point3(1, i, 0), lowerRight + new Point3(-1, i, 0), wood1);
                    world.FillLine(upperLeft + new Point3(1, i, 0), upperRight + new Point3(-1, i, 0), wood1);
                }
                else
                {
                    world.FillLine(lowerLeft + new Point3(0, i, 1), upperLeft + new Point3(0, i, -1), wood1);
                    world.FillLine(lowerRight + new Point3(0, i, 1), upperRight + new Point3(0, i, -1), wood1);
                    world.FillLine(lowerLeft + new Point3(-1, i, 0), lowerRight + new Point3(1, i, 0), wood0);
                    world.FillLine(upperLeft + new Point3(-1, i, 0), upperRight + new Point3(1, i, 0), wood0);
                }
            }
            if (levelCount % 2 == 0)
            {
                for (int i = -1; i < side / 2 + 1; ++i)
                {
                    if (i >= 0)
                    {
                        world.FillLine(
                            lowerLeft + new Point3(0, levelCount + i, i - 1),
                            upperLeft + new Point3(0, levelCount + i, -i + 1),
                            wood0);
                        world.FillLine(
                            lowerRight + new Point3(0, levelCount + i, i - 1),
                            upperRight + new Point3(0, levelCount + i, -i + 1),
                            wood0);
                    }
                    if (hasRoof)
                    {
                        world.FillLine(
                            lowerLeft + new Point3(-1, levelCount + i, i),
                            lowerRight + new Point3(1, levelCount + i, i),
                            roof);
                        world.FillLine(
                            upperLeft + new Point3(-1, levelCount + i, -i),
                            upperRight + new Point3(1, levelCount + i, -i),
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
                            lowerLeft + new Point3(i - 1, levelCount + i, 0),
                            lowerRight + new Point3(-i + 1, levelCount + i, 0),
                            wood0);
                        world.FillLine(
                            upperLeft + new Point3(i - 1, levelCount + i, 0),
                            upperRight + new Point3(-i + 1, levelCount + i, 0),
                            wood0);
                    }
                    if (hasRoof)
                    {
                        world.FillLine(
                            lowerLeft + new Point3(i, levelCount + i, -1),
                            upperLeft + new Point3(i, levelCount + i, 1),
                            roof);
                        world.FillLine(
                            lowerRight + new Point3(-i, levelCount + i, -1),
                            upperRight + new Point3(-i, levelCount + i, 1),
                            roof);
                    }
                }
            }
        }
    }
}
