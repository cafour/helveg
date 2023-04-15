using Helveg.CSharp.Projects;
using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record RoslynMinerOptions : MinerOptions
{
    public SymbolAnalysisScope ProjectSymbolAnalysisScope { get; init; }

    public SymbolAnalysisScope ExternalSymbolAnalysisScope { get; init; }

    public ImmutableDictionary<string, string> MSBuildProperties { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public RoslynMinerOptions()
    {
    }
}
