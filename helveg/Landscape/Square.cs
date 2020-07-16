using Helveg.Render;

namespace Helveg.Landscape
{
    public struct Square
    {
        public Square(Point3 center, int side)
        {
            LowerLeft = center + new Point3(-side / 2, 0, -side / 2);
            LowerRight = center + new Point3(side / 2 + side % 2, 0, -side / 2);
            UpperLeft = center + new Point3(-side / 2, 0, side / 2 + side % 2);
            UpperRight = center + new Point3(side / 2 + side % 2, 0, side / 2 + side % 2);
        }

        public Point3 LowerLeft { get; }
        public Point3 LowerRight { get; }
        public Point3 UpperLeft { get; }
        public Point3 UpperRight { get; }
    }
}
