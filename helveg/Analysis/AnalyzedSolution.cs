using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Analysis
{
    public struct AnalyzedSolution : IEquatable<AnalyzedSolution>, ISeedable
    {
        public readonly string Path;
        public readonly string Name;
        public readonly ImmutableDictionary<string, AnalyzedProject> Projects;

        public AnalyzedSolution(string path, string name, ImmutableDictionary<string, AnalyzedProject> projects)
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
            return Path.Equals(other.Path)
                && Name.Equals(other.Name)
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

        public Graph GetGraph()
        {
            var names = Projects.Keys.ToArray();
            var weights = new float[names.Length * (names.Length - 1)];
            var weightIndex = 0;
            for (int from = 0; from < names.Length; ++from)
            {
                for (int to = from + 1; to < names.Length; ++to, ++weightIndex)
                {
                    weights[weightIndex] = Projects[names[from]].ProjectReferences.Contains(names[to])
                        || Projects[names[to]].ProjectReferences.Contains(names[from])
                        ? 1
                        : 0;
                }
            }

            var self = this;
            var sizes = names.Select(n => (float)self.Projects[n].Types.Count).ToArray();
            return new Graph(new Vector2[Projects.Count], weights, names, sizes);
        }
    }
}
