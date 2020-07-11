using System;
using System.Collections.Immutable;
using System.Linq;

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
                    if (friendIndex != -1)
                    {
                        matrix[i, friendIndex] += weight;
                    }
                }
            }
            return (ids, matrix);
        }

        public int GetSeed()
        {
            return Checksum.GetCrc32(Name) ^ ISeedable.Arbitrary;
        }
    }
}
