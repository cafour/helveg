using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelModuleReferenceCS : HelReferenceCS, IInvalidable<HelModuleReferenceCS>
{
    public static HelModuleReferenceCS Invalid { get; } = new();
}

public record HelModuleCS : HelDefinitionCS, IInvalidable<HelModuleCS>
{
    public static HelModuleCS Invalid { get; } = new();

    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelAssemblyReferenceCS> ReferencedAssemblies { get; init; }
        = ImmutableArray<HelAssemblyReferenceCS>.Empty;

    public HelAssemblyReferenceCS ContainingAssembly { get; init; } = HelAssemblyReferenceCS.Invalid;

    public override HelModuleReferenceCS GetReference()
    {
        return new() { Token = Token, IsResolved = true };
    }
}
