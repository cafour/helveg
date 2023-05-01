using System;
using Helveg.Sample.Diagnostics.Manufacturing;

namespace Helveg.Sample.Diagnostics.Data
{
    public struct CheesePiece : IData
    {
        public Guid Id { get; }
        public CheeseKind Kind { get; set; }
        public ICheeseUnit Amount { get; set; }
        public ICheeseFactory Manufacturer { get; set; }
    }
}
