using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Analysis
{
    public struct AnalyzedProject : IEquatable<AnalyzedProject>, ISeedable
    {
        public readonly string Path;
        public readonly string Name;
        public readonly ImmutableDictionary<AnalyzedTypeId, AnalyzedType> Types;
        public readonly ImmutableHashSet<string> ProjectReferences;
        public readonly ImmutableHashSet<string> PackageReferences;
        public readonly DateTime LastWriteTime;

        public AnalyzedProject(
            string path,
            string name,
            ImmutableDictionary<AnalyzedTypeId, AnalyzedType> types,
            ImmutableHashSet<string> projectReferences,
            ImmutableHashSet<string> packageReferences,
            DateTime lastWriteTime)
        {
            Path = path;
            Name = name;
            Types = types;
            ProjectReferences = projectReferences;
            PackageReferences = packageReferences;
            LastWriteTime = lastWriteTime;
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
            return Path.Equals(other.Path)
                && Name.Equals(other.Name)
                && Enumerable.SequenceEqual(Types, other.Types)
                && LastWriteTime.Equals(other.LastWriteTime);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, Types.Count);
        }

        public override string? ToString()
        {
            return $"{Name} [prj,typ={Types.Count},ref={ProjectReferences.Count},"
                + $"pkg={PackageReferences.Count},lwt={LastWriteTime},pth={Path}]";
        }

        public (Graph graph, AnalyzedTypeId[] ids) GetGraph()
        {
            var matrix = new int[Types.Count, Types.Count];
            var ids = Types.Keys.OrderBy(k => k.ToString()).ToArray();
            var sizes = new float[Types.Count];
            for (int i = 0; i < ids.Length; ++i)
            {
                var type = Types[ids[i]];
                sizes[i] = type.MemberCount;
                foreach (var (friend, weight) in type.Relations)
                {
                    var friendIndex = Array.IndexOf(ids, friend);
                    if (friendIndex != -1)
                    {
                        matrix[i, friendIndex] += weight;
                    }
                }
            }
            var graph = new Graph(
                positions: new Vector2[Types.Count],
                weights: Graph.UndirectWeights(matrix),
                labels: ids.Select(id => id.ToString()).ToArray(),
                sizes: sizes);
            return (graph, ids);
        }

        public int GetSeed()
        {
            return Checksum.GetCrc32(Name) ^ ISeedable.Arbitrary;
        }
    }
}
