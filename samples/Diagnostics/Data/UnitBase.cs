using System;

namespace Helveg.Sample.Diagnostics.Data
{
    public abstract class UnitBase : IData, ICheeseUnit
    {
        public Guid Id { get; }
        public abstract int Weight { get; }
        public abstract float Saltiness { get; }
        public abstract float MilkContent { get; }
        public abstract int HoleCount { get; }
        public abstract int Price { get; }
        public abstract int Cost { get; }
        public abstract int Mana { get; }
    }
}
