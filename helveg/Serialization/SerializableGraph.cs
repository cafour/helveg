using System.Collections.Generic;
using System.Numerics;

namespace Helveg.Serialization
{
    public class SerializableGraph
    {
        public string? Name { get; set; }
        public Dictionary<string, Vector2>? Positions { get; set; }
    }
}
