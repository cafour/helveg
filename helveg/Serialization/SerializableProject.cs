using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Helveg.Analysis;

namespace Helveg.Serialization
{
    public class SerializableProject
    {
        public string? CsprojPath { get; set; }

        public string? Name { get; set; }

        public Dictionary<string, SerializableType>? Types { get; set; }

        public HashSet<string>? ProjectReferences { get; set; }

        public HashSet<string>? PackageReferences { get; set; }

        public static SerializableProject FromAnalyzed(string? csprojPath, AnalyzedProject project)
        {
            return new SerializableProject
            {
                CsprojPath = csprojPath,
                Name = project.Name,
                Types = project.Types.ToDictionary(
                    p => p.Key.ToString(),
                    p => SerializableType.FromAnalyzed(p.Value)),
                ProjectReferences = project.ProjectReferences.ToHashSet(),
                PackageReferences = project.PackageReferences.ToHashSet()
            };
        }

        public AnalyzedProject ToAnalyzed()
        {
            if (Name is null || Types is null || ProjectReferences is null || PackageReferences is null)
            {
                throw new NullReferenceException();
            }

            return new AnalyzedProject(
                name: Name,
                types: Types.ToImmutableDictionary(p => AnalyzedTypeId.Parse(p.Key), p => p.Value.ToAnalyzed()),
                projectReferences: ProjectReferences.ToImmutableHashSet(),
                packageReferences: PackageReferences.ToImmutableHashSet());
        }
    }
}
