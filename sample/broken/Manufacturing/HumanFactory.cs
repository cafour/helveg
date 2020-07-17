using System;
using System.Collections.Generic;
using Helveg.Sample.Data;

namespace Helveg.Sample.Manufacturing
{
    public class HumanFactory : ICheeseFactory
    {
        private readonly List<HumanWorker> employees;

        public void CloseDown()
        {
            throw new InvalidOperationException("The cheese must flow on.");
        }

        public void Employ(ICheeseWorker worker)
        {
            if(worker is HumanWorker human)
            {
                employees.Add(human);
            }
            throw new InvalidOperationException("This factory is for human only!");
        }

        public void Fire(ICheeseWorker worker)
        {
            if (worker is HumanWorker human)
            {
                employees.Remove(human);
            }
        }

        public CheesePiece MakeCheese()
        {
            return new CheesePiece
            {
                Amount = new WheelUnit(),
                Kind = CheeseKind.Gouda,
                Manufacturer = this
            };
        }

        public ICheeseUnit ShipCheese(CheesePiece piece)
        {
            return piece.Amount;
        }
    }
}
