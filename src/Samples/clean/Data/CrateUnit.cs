namespace Helveg.Sample.Data
{
    public class CreateUnit : UnitBase
    {
        public override int Weight {get; } = 25;
        public override float Saltiness {get; } = 0.67f;
        public override float MilkContent {get; } = 0.2f;
        public override int HoleCount {get; } = 11;
        public override int Price {get; } = 99;
        public override int Cost {get; } = 80;
    }
}
