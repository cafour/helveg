using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelModuleCS : HelDefinitionCS, IInvalidable<HelModuleCS>
{
    public static HelModuleCS Invalid { get; } = new();

    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelReferenceCS> ReferencedAssemblies { get; init; }
        = ImmutableArray<HelReferenceCS>.Empty;

    public HelReferenceCS ContainingAssembly { get; init; } = HelReferenceCS.Invalid;
}
