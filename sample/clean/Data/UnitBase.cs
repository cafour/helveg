using System;

namespace Helveg.Sample.Data
{
    public abstract class UnitBase : IData, ICheeseUnit
    {
        public Guid Id { get; }

        public abstract int GetWeight();
    }
}
