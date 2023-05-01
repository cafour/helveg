using Helveg.Sample.Diagnostics.Data;

namespace Helveg.Sample.Diagnostics.Manufacturing
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
