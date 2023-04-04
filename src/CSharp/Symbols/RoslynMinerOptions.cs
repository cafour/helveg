using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record RoslynMinerOptions
{
    public static readonly ImmutableArray<string> DefaultBuildTargets = ImmutableArray.Create("Restore", "Build");

    public bool IncludeExternalSymbols { get; init; }

    public ImmutableDictionary<string, string> MSBuildProperties { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public ImmutableArray<string> MSBuildTargets { get; init; }
        = DefaultBuildTargets;
}
