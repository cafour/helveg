using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelModuleReferenceCS : HelReferenceCS, IInvalidable<HelModuleReferenceCS>
{
    public static HelModuleReferenceCS Invalid { get; } = new();

    public HelAssemblyReferenceCS ContainingAssembly { get; init; } = HelAssemblyReferenceCS.Invalid;
}

public record HelModuleCS : HelDefinitionCS<HelModuleReferenceCS>, IInvalidable<HelModuleCS>
{
    public static HelModuleCS Invalid { get; } = new();

    public override HelModuleReferenceCS Reference => new()
    {
        Token = Token,
        Name = Name,
        ContainingAssembly = ContainingAssembly
    };

    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelAssemblyReferenceCS> ReferencedAssemblies { get; init; }
        = ImmutableArray<HelAssemblyReferenceCS>.Empty;

    public HelAssemblyReferenceCS ContainingAssembly { get; init; } = HelAssemblyReferenceCS.Invalid;
}
