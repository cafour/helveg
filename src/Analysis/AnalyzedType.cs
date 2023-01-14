using System;
using System.Collections.Immutable;
using System.Linq;

namespace Helveg.Analysis
{
    public struct AnalyzedType : IEquatable<AnalyzedType>
    {
        public readonly AnalyzedTypeId Id;
        public readonly AnalyzedTypeKind Kind;
        public readonly Diagnosis Health;
        public readonly int Size;
        public readonly ImmutableDictionary<AnalyzedTypeId, int> Relations;
        public readonly int Family;

        public AnalyzedType(
            AnalyzedTypeId id,
            AnalyzedTypeKind kind,
            Diagnosis health,
            int size,
            ImmutableDictionary<AnalyzedTypeId, int> relations,
            int family)
        {
            Id = id;
            Kind = kind;
            Health = health;
            Size = size;
            Relations = relations;
            Family = family;
        }

        public static bool operator ==(AnalyzedType left, AnalyzedType right)
            => left.Equals(right);

        public static bool operator !=(AnalyzedType left, AnalyzedType right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
        {
            if (obj is AnalyzedType member)
            {
                return Equals(member);
            }
            return false;
        }

        public bool Equals(AnalyzedType other)
        {
            return Id.Equals(other.Id)
                && Kind.Equals(other.Kind)
                && Health.Equals(other.Health)
                && Size.Equals(other.Size)
                && !Relations.Except(other.Relations).Any()
                && Family.Equals(other.Family);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Health, Size, Relations.Count, Family);
        }

        public override string? ToString()
        {
            return $"{Id} [typ,{Kind},{Health},mmb={Size},rel={Relations.Count}]";
        }

        public int GetSeed()
        {
            return Id.GetSeed();
        }

        public AnalyzedType WithHealth(Diagnosis health)
        {
            return new AnalyzedType(Id, Kind, health, Size, Relations, Family);
        }

        public AnalyzedType WithFamily(int family)
        {
            return new AnalyzedType(Id, Kind, Health, Size, Relations, family);
        }
    }
}
