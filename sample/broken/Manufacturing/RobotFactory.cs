using System;
using System.Collections.Generic;
using Helveg.Sample.Data;

namespace Helveg.Sample.Manufacturing
{
    public class RobotFactory : ICheeseFactory
    {
        private List<ICheeseWorker> employees = new List<ICheeseWorker>();
        private bool isClosed = false;

        public void CloseDown()
        {
            employees.Clear();
            isClosed = true;
        }

        public void Employ(ICheeseWorker worker)
        {
            if (isClosed)
            {
                throw new InvalidOperationException("Factory is closed.");
            }
            employees.Add(worker);
        }

        public void Fire(ICheeseWorker worker)
        {
            if (isClosed)
            {
                throw new InvalidOperationException("Factory is closed.");
            }
            employees.Remove(worker);
        }

        public CheesePiece MakeCheese()
        {
            if (isClosed)
            {
                throw new InvalidOperationException("Factory is closed.");
            }
            return new CheesePiece
            {
                Amount = new TruckUnit(),
                Kind = CheeseKind.Eidam,
                Manufacturer = this
            };
        }

        public ICheeseUnit ShipCheese(CheesePiece piece)
        {
            if (isClosed)
            {
                throw new InvalidOperationException("Factory is closed.");
            }
            return piece.Amount;
        }
    }
}
