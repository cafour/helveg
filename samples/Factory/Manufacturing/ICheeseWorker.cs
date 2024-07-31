namespace Helveg.Sample.Factory.Manufacturing
{
    public interface ICheeseWorker
    {
        string Name { get; set; }

        bool IsDead { get; set;}

        void Die()
        {
            IsDead = true;
        }
    }
}
