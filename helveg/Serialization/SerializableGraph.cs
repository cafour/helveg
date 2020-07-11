using System;
using System.Collections.Generic;
using System.Numerics;

namespace Helveg.Serialization
{
    public class SerializableGraph
    {
        public DateTime TimeStamp { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, Vector2>? Positions { get; set; }
    }
}
