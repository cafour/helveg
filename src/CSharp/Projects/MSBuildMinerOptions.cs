using System.Collections.Immutable;

namespace Helveg.CSharp.Projects;

public record MSBuildMinerOptions : MinerOptions
{
    public ImmutableDictionary<string, string> MSBuildProperties { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public bool IncludeExternalDependencies { get; init; } = true;
}
