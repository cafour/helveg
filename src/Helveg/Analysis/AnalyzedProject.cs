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
                && !Types.Except(other.Types).Any()
                && PackageReferences.SetEquals(other.PackageReferences)
                && ProjectReferences.SetEquals(other.ProjectReferences)
                && LastWriteTime.Equals(other.LastWriteTime);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Path,
                Name,
                Types.Count,
                PackageReferences.Count,
                ProjectReferences.Count,
                LastWriteTime);
        }

        public override string? ToString()
        {
            return $"{Name} [prj,typ={Types.Count},ref={ProjectReferences.Count},"
                + $"pkg={PackageReferences.Count},lwt={LastWriteTime},pth={Path}]";
        }

        public int GetSeed()
        {
            return Checksum.GetCrc32(Name) ^ ISeedable.Arbitrary;
        }

        public AnalyzedProject WithTypes(ImmutableDictionary<AnalyzedTypeId, AnalyzedType> types)
        {
            return new AnalyzedProject(Path, Name, types, ProjectReferences, PackageReferences, LastWriteTime);
        }
    }
}
