using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelModuleCS : HelSymbolBaseCS
{
    public static readonly HelModuleCS Invalid = new();

    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelAssemblyIdCS> ReferencedAssemblies { get; init; } = ImmutableArray<HelAssemblyIdCS>.Empty;
}
