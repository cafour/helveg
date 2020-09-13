using System;
using System.Collections.Generic;

namespace Helveg.Sample.Dal
{
    public class TeaDrinker
    {
        public Guid Id { get; set; }

        public List<CupOfTea> Cups { get; set; }
    }
}
