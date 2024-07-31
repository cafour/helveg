using System;

namespace Helveg.Sample.Factory.Manufacturing
{
    public class RobotWorker : ICheeseWorker
    {
        public string Name { get; set; }
        public bool IsDead { get; set; }

        public void Die()
        {
            throw new InvalidOperationException("Robots can't die.");
        }
    }
}
