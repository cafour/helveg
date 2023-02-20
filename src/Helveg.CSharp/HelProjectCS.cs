using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelProjectReferenceCS : HelReferenceCS, IInvalidable<HelProjectReferenceCS>
{
    public static HelProjectReferenceCS Invalid { get; } = new();
}

public record HelProjectCS : HelDefinitionCS, IInvalidable<HelProjectCS>
{
    public static HelProjectCS Invalid { get; } = new();

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<HelReferenceCS> ProjectDependencies { get; init; }
        = ImmutableArray<HelReferenceCS>.Empty;

    public ImmutableArray<HelReferenceCS> PackageDependencies { get; init; }
        = ImmutableArray<HelReferenceCS>.Empty;

    public HelAssemblyCS Assembly { get; init; } = HelAssemblyCS.Invalid;

    public HelSolutionReferenceCS ContainingSolution { get; init; } = HelSolutionReferenceCS.Invalid;

    public override HelProjectReferenceCS GetReference()
    {
        return new() { Token = Token };
    }
}
