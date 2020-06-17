using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Helveg.Analysis
{
    public struct AnalyzedProject : IEquatable<AnalyzedProject>
    {
        public readonly string Name;
        public readonly ImmutableDictionary<AnalyzedTypeId, AnalyzedType> Types;

        public AnalyzedProject(
            string name,
            ImmutableDictionary<AnalyzedTypeId, AnalyzedType> types)
        {
            Name = name;
            Types = types;
        }

        public static bool operator ==(AnalyzedProject left, AnalyzedProject right)
            => left.Equals(right);

        public static bool operator !=(AnalyzedProject left, AnalyzedProject right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
        {
            if (obj is AnalyzedType member)
            {
                return Equals(member);
            }
            return false;
        }

        public bool Equals(AnalyzedProject other)
        {
            return Name.Equals(other.Name)
                && Enumerable.SequenceEqual(Types, other.Types);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Types.Count);
        }

        public override string? ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[P");
            sb.Append(" #");
            sb.Append(Types.Count);
            sb.Append("] ");
            sb.Append(Name);
            return sb.ToString();
        }

        public (AnalyzedTypeId[] names, int[,] weights) GetWeightMatrix()
        {
            var matrix = new int[Types.Count, Types.Count];
            var ids = Types.Keys.OrderBy(k => k.ToString()).ToArray();
            for (int i = 0; i < ids.Length; ++i)
            {
                var type = Types[ids[i]];
                foreach (var (friend, weight) in type.Relations)
                {
                    var friendIndex = Array.IndexOf(ids, friend);
                    matrix[i, friendIndex] += weight;
                }
            }
            return (ids, matrix);
        }
    }
}
