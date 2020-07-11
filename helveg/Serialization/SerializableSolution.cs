using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Helveg.Analysis;

namespace Helveg.Serialization
{
    public class SerializableSolution
    {
        public string? Path { get; set; }

        public string? Name { get; set; }

        public Dictionary<string, SerializableProject>? Projects { get; set; }

        public static SerializableSolution FromAnalyzed(string? path, AnalyzedSolution solution)
        {
            return new SerializableSolution
            {
                Path = path,
                Name = solution.Name,
                Projects = solution.Projects.ToDictionary(
                    p => p.Key,
                    p => SerializableProject.FromAnalyzed(null, p.Value))
            };
        }

        public AnalyzedSolution ToAnalyzed()
        {
            if (Name is null || Projects is null)
            {
                throw new NullReferenceException();
            }

            return new AnalyzedSolution(
                name: Name,
                projects: Projects.ToImmutableDictionary(
                    p => p.Key,
                    p => p.Value.ToAnalyzed()));
        }
    }
}
