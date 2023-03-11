using Helveg.Render;

namespace Helveg.Landscape
{
    public static class ConstructionSite
    {
        public static void Draw(
            WorldBuilder world,
            Block corner,
            Block beam,
            Point3 position,
            int side)
        {
            var square = new Square(position + new Point3(0, 1, 0), side);

            world.FillLine(square.LowerLeft, square.LowerRight, beam);
            world.FillLine(square.LowerRight, square.UpperRight, beam);
            world.FillLine(square.UpperRight, square.UpperLeft, beam);
            world.FillLine(square.UpperLeft, square.LowerLeft, beam);
            world.FillLine(square.UpperLeft, square.LowerRight, beam);
            world.FillLine(square.LowerLeft, square.UpperRight, beam);

            world.FillVolume(
                square.LowerLeft + new Point3(0, 1, 0),
                square.LowerLeft + new Point3(-1, -3, -1),
                corner);
            world.FillVolume(
                square.LowerRight + new Point3(0, 1, 0),
                square.LowerRight + new Point3(1, -3, -1),
                corner);
            world.FillVolume(
                square.UpperLeft + new Point3(0, 1, 0),
                square.UpperLeft + new Point3(-1, -3, 1),
                corner);
            world.FillVolume(
                square.UpperRight + new Point3(0, 1, 0),
                square.UpperRight + new Point3(1, -3, 1),
                corner);
        }
    }
}
