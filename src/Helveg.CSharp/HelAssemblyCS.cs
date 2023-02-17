using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelAssemblyCS : HelDefinitionCS, IInvalidable<HelAssemblyCS>
{
    public static HelAssemblyCS Invalid { get; } = new();

    public HelAssemblyIdCS Identity { get; init; } = HelAssemblyIdCS.Invalid;

    public ImmutableArray<HelModuleCS> Modules { get; init; } = ImmutableArray<HelModuleCS>.Empty;

    public HelReferenceCS? ContainingProject { get; init; }

    public HelReferenceCS? ContainingPackage { get; init; }
}
