using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelAssemblyCS : HelSymbolBaseCS
{
    public static readonly HelAssemblyCS Invalid = new();

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelModuleCS> Modules { get; init; } = ImmutableArray<HelModuleCS>.Empty;
}
