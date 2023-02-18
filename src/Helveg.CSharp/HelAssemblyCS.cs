using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelAssemblyReferenceCS : HelReferenceCS, IInvalidable<HelAssemblyReferenceCS>
{
    public static HelAssemblyReferenceCS Invalid { get; } = new();
}

public record HelAssemblyCS : HelDefinitionCS, IInvalidable<HelAssemblyCS>
{
    public static HelAssemblyCS Invalid { get; } = new();

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public ImmutableArray<HelModuleCS> Modules { get; init; } = ImmutableArray<HelModuleCS>.Empty;

    public HelProjectReferenceCS? ContainingProject { get; init; }

    public HelPackageReferenceCS? ContainingPackage { get; init; }

    public override HelAssemblyReferenceCS GetReference()
    {
        return new() { Token = Token };
    }
}
