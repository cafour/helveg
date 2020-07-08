using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Helveg.Landscape;

namespace Helveg.Analysis
{
    public struct AnalyzedType : IEquatable<AnalyzedType>, ISeedable
    {
        public readonly AnalyzedTypeId Id;
        public readonly AnalyzedTypeKind Kind;
        public readonly Diagnosis Health;
        public readonly ImmutableArray<AnalyzedMember> Members;
        public readonly ImmutableDictionary<AnalyzedTypeId, int> Relations;


        public AnalyzedType(
            AnalyzedTypeId id,
            AnalyzedTypeKind kind,
            Diagnosis health,
            ImmutableArray<AnalyzedMember> members,
            ImmutableDictionary<AnalyzedTypeId, int> relations)
        {
            Id = id;
            Kind = kind;
            Health = health;
            Members = members;
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
                && Enumerable.SequenceEqual(Members, other.Members)
                && Enumerable.SequenceEqual(Relations, other.Relations);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Health, Members.Length, Relations.Count);
        }

        public override string? ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[T");
            sb.Append(Health.ToDebuggerString());
            sb.Append(Kind.ToDebuggerString());
            sb.Append(" #");
            sb.Append(Members.Length);
            sb.Append(" |");
            sb.Append(Relations.Count);
            sb.Append("] ");
            sb.Append(Id);
            return sb.ToString();
        }

        public int GetSeed()
        {
            return Checksum.GetCrc32(Id.Name) ^ ISeedable.Arbitrary;
        }
    }
}
