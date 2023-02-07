using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelAssemblyReferenceCS : HelReferenceCS, IInvalidable<HelAssemblyReferenceCS>
{
    public static HelAssemblyReferenceCS Invalid { get; } = new();

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public HelProjectReferenceCS ContainingProject { get; init; } = HelProjectReferenceCS.Invalid;
}

public record HelAssemblyCS : HelDefinitionCS<HelAssemblyReferenceCS>, IInvalidable<HelAssemblyCS>
{
    public static HelAssemblyCS Invalid { get; } = new();

    public override HelAssemblyReferenceCS Reference => new()
    {
        Identity = Identity,
        Name = Name,
        ContainingProject = ContainingProject
    };

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public HelNamespaceCS GlobalNamespace { get; init; } = HelNamespaceCS.Invalid;

    public ImmutableArray<HelModuleCS> Modules { get; init; } = ImmutableArray<HelModuleCS>.Empty;

    public HelProjectReferenceCS ContainingProject { get; init; } = HelProjectReferenceCS.Invalid;
}
