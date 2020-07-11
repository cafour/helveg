using System;
using System.Collections.Immutable;
using System.Linq;

namespace Helveg.Analysis
{
    public struct AnalyzedType : IEquatable<AnalyzedType>, ISeedable
    {
        public readonly AnalyzedTypeId Id;
        public readonly AnalyzedTypeKind Kind;
        public readonly Diagnosis Health;
        public readonly int MemberCount;
        public readonly ImmutableDictionary<AnalyzedTypeId, int> Relations;

        public AnalyzedType(
            AnalyzedTypeId id,
            AnalyzedTypeKind kind,
            Diagnosis health,
            int memberCount,
            ImmutableDictionary<AnalyzedTypeId, int> relations)
        {
            Id = id;
            Kind = kind;
            Health = health;
            MemberCount = memberCount;
            Relations = relations;
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
                && MemberCount.Equals(other.MemberCount)
                && Enumerable.SequenceEqual(Relations, other.Relations);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Health, MemberCount, Relations.Count);
        }

        public override string? ToString()
        {
            return $"{Id} [typ,{Kind},{Health},mmb={MemberCount},rel={Relations.Count}]";
        }

        public int GetSeed()
        {
            return Id.GetSeed();
        }
    }
}
