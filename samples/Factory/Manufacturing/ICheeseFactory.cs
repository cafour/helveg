using Helveg.Sample.Factory.Data;

namespace Helveg.Sample.Factory.Manufacturing
{
    public interface ICheeseFactory
    {
        CheesePiece MakeCheese();
        void Employ(ICheeseWorker worker);
        void Fire(ICheeseWorker worker);
        ICheeseUnit ShipCheese(CheesePiece piece);
        void CloseDown();
    }
}
