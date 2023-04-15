namespace Helveg.Sample.Data
{
    public interface ICheeseUnit
    {
        int Weight { get; }

        float Saltiness { get; }

        float MilkContent { get; }

        int HoleCount { get; }

        int Price { get; }

        int Cost { get; }
    }
}
