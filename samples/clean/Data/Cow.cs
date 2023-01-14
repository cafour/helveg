using System;

namespace Helveg.Sample.Data
{
    public struct Cow : IData
    {
        public Guid Id { get; }
        public string Name { get; set; }
    }
}
