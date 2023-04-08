using Helveg.CSharp.Projects;
using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record RoslynMinerOptions : MinerOptions
{
    public bool IncludeExternalSymbols { get; init; }

    public ImmutableDictionary<string, string> MSBuildProperties { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public RoslynMinerOptions()
    {
    }
}
