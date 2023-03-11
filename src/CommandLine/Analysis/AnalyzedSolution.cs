using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Analysis
{
    public struct AnalyzedSolution : IEquatable<AnalyzedSolution>, ISeedable
    {
        public readonly string? Path;
        public readonly string? Name;
        public readonly ImmutableDictionary<string, AnalyzedProject> Projects;

        public AnalyzedSolution(string? path, string? name, ImmutableDictionary<string, AnalyzedProject> projects)
        {
            Path = path;
            Name = name;
            Projects = projects;
        }

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
            return (Path is null && other.Path is null) || (Path is not null && Path.Equals(other.Path))
                && (Name is null && other.Name is null ) || (Name is not null && Name.Equals(other.Name))
                && Enumerable.SequenceEqual(Projects, other.Projects);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, Projects.Count);
        }

        public override string? ToString()
        {
            return $"{Name} [sln,prj={Projects.Count},pth={Path}]";
        }

        public int GetSeed()
        {
            return Checksum.GetCrc32(Name) ^ ISeedable.Arbitrary;
        }
    }
}
