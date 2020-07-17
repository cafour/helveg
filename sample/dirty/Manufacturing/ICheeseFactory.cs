using Helveg.Sample.Data;

namespace Helveg.Sample.Manufacturing
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
