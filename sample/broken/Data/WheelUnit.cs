namespace Helveg.Sample.Data
{
    public class WheelUnit : UnitBase
    {
        public override int Weight { get; } = 10;
        public override float Saltiness { get; } = 0.4f;
        public override float MilkContent { get; } = 0.9f;
        public override int HoleCount { get; } = -10;
        public override int Price { get; } = 50;
    }
}
