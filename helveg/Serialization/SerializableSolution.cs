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

        public static SerializableSolution FromAnalyzed(AnalyzedSolution solution)
        {
            return new SerializableSolution
            {
                Path = solution.Path,
                Name = solution.Name,
                Projects = solution.Projects.ToDictionary(
                    p => p.Key,
                    p => SerializableProject.FromAnalyzed(p.Value))
            };
        }

        public AnalyzedSolution ToAnalyzed()
        {
            if (Path is null || Name is null || Projects is null)
            {
                throw new NullReferenceException();
            }

            return new AnalyzedSolution(
                path: Path,
                name: Name,
                projects: Projects.ToImmutableDictionary(
                    p => p.Key,
                    p => p.Value.ToAnalyzed()));
        }
    }
}
