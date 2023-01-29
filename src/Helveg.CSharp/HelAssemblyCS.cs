using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelAssemblyCS : HelSymbolBaseCS
{
    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelModuleCS> Modules { get; init; } = ImmutableArray<HelModuleCS>.Empty;
}
