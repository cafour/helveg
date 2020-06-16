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

        public static SerializableProject FromAnalyzed(string csprojPath, AnalyzedProject project)
        {
            return new SerializableProject
            {
                CsprojPath = csprojPath,
                Name = project.Name,
                Types = project.Types.ToDictionary(
                    p => p.Key.ToString(),
                    p => SerializableType.FromAnalyzed(p.Value))
            };
        }

        public AnalyzedProject ToAnalyzed()
        {
            if (Name is null || Types is null)
            {
                throw new NullReferenceException();
            }
            
            return new AnalyzedProject(
                name: Name,
                types: Types.ToImmutableDictionary(p => AnalyzedTypeId.Parse(p.Key), p => p.Value.ToAnalyzed()));
        }
    }
}
