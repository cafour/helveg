using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelAssemblyReferenceCS : HelReferenceCS, IInvalidable<HelAssemblyReferenceCS>
{
    public static HelAssemblyReferenceCS Invalid { get; } = new();

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public HelProjectReferenceCS? ContainingProject { get; init; }

    public HelPackageReferenceCS? ContainingPackage { get; init; }
}

public record HelAssemblyCS : HelDefinitionCS<HelAssemblyReferenceCS>, IInvalidable<HelAssemblyCS>
{
    public static HelAssemblyCS Invalid { get; } = new();

    public override HelAssemblyReferenceCS Reference => new()
    {
        DefinitionToken = DefinitionToken,
        Identity = Identity,
        Name = Name,
        ContainingProject = ContainingProject,
        ContainingPackage = ContainingPackage
    };

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public ImmutableArray<HelModuleCS> Modules { get; init; } = ImmutableArray<HelModuleCS>.Empty;

    public HelProjectReferenceCS? ContainingProject { get; init; }

    public HelPackageReferenceCS? ContainingPackage { get; init; }
}
