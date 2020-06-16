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
        public List<SerializableMember>? Members { get; set; }
        public Dictionary<string, int>? Relations { get; set; }

        public static SerializableType FromAnalyzed(AnalyzedType type)
        {
            return new SerializableType
            {
                Id = type.Id.ToString(),
                Kind = type.Kind,
                Health = type.Health,
                Members = type.Members.Select(SerializableMember.FromAnalyzed).ToList(),
                Relations = type.Relations.ToDictionary(p => p.Key.ToString(), p => p.Value)
            };
        }

        public AnalyzedType ToAnalyzed()
        {
            if (Id is null || Members is null || Relations is null)
            {
                throw new NullReferenceException();
            }

            return new AnalyzedType(
                id: AnalyzedTypeId.Parse(Id),
                kind: Kind,
                health: Health,
                members: Members.Select(m => m.ToAnalyzed()).ToImmutableArray(),
                relations: Relations.ToImmutableDictionary(p => AnalyzedTypeId.Parse(p.Key), p => p.Value));
        }
    }
}
