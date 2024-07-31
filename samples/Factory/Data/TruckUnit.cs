namespace Helveg.Sample.Factory.Data
{
    public class TruckUnit : UnitBase
    {
        public override int Weight { get; } = 5000;
        public override float Saltiness { get; } = 0.90f;
        public override float MilkContent { get; } = 0.4f;
        public override int HoleCount { get; } = 11000;
        public override int Price { get; } = 99999;
        public override int Cost { get; } = 10000;
    }
}
