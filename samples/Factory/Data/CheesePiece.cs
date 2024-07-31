using System;
using Helveg.Sample.Factory.Manufacturing;

namespace Helveg.Sample.Factory.Data
{
    public struct CheesePiece : IData
    {
        public Guid Id { get; }
        public CheeseKind Kind { get; set; }
        public ICheeseUnit Amount { get; set; }
        public ICheeseFactory Manufacturer { get; set; }
    }
}
