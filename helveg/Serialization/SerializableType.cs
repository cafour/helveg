using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Helveg.Analysis;

namespace Helveg.Serialization
{
    public class SerializableType
    {
        public string? Id { get; set; }
        public AnalyzedTypeKind Kind { get; set; }
        public Diagnosis Health { get; set; }
        public int Size { get; set; }
        public Dictionary<string, int>? Relations { get; set; }
        public int Family { get; set; }

        public static SerializableType FromAnalyzed(AnalyzedType type)
        {
            return new SerializableType
            {
                Id = type.Id.ToString(),
                Kind = type.Kind,
                Health = type.Health,
                Size = type.Size,
                Relations = type.Relations.ToDictionary(p => p.Key.ToString(), p => p.Value),
                Family = type.Family,
            };
        }

        public AnalyzedType ToAnalyzed()
        {
            if (Id is null || Relations is null)
            {
                throw new NullReferenceException();
            }

            return new AnalyzedType(
                id: AnalyzedTypeId.Parse(Id),
                kind: Kind,
                health: Health,
                size: Size,
                relations: Relations.ToImmutableDictionary(p => AnalyzedTypeId.Parse(p.Key), p => p.Value),
                family: Family);
        }
    }
}
