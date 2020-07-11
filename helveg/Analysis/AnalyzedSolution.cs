using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Helveg.Landscape;

namespace Helveg.Analysis
{
    public struct AnalyzedSolution : IEquatable<AnalyzedSolution>, ISeedable
    {
        public readonly string Name;
        public readonly ImmutableDictionary<string, AnalyzedProject> Projects;

        public static bool operator ==(AnalyzedSolution left, AnalyzedSolution right)
            => left.Equals(right);

        public static bool operator !=(AnalyzedSolution left, AnalyzedSolution right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
        {
            if (obj is AnalyzedSolution sln)
            {
                return Equals(sln);
            }
            return false;
        }

        public bool Equals(AnalyzedSolution other)
        {
            return Name.Equals(other.Name)
                && Enumerable.SequenceEqual(Projects, other.Projects);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Projects.Count);
        }

        public override string? ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[S");
            sb.Append(" #");
            sb.Append(Projects.Count);
            sb.Append("] ");
            sb.Append(Name);
            return sb.ToString();
        }

        public int GetSeed()
        {
            return Checksum.GetCrc32(Name) ^ ISeedable.Arbitrary;
        }
    }
}
