using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelProjectReferenceCS : HelReferenceCS, IInvalidable<HelProjectReferenceCS>
{
    public static HelProjectReferenceCS Invalid { get; } = new();

    public string? FullName { get; init; }

    public HelSolutionReferenceCS ContainingSolution { get; init; } = HelSolutionReferenceCS.Invalid;
}

public record HelProjectCS : HelDefinitionCS<HelProjectReferenceCS>, IInvalidable<HelProjectCS>
{
    public static HelProjectCS Invalid { get; } = new();

    public override HelProjectReferenceCS Reference => new()
    {
        Name = Name,
        FullName = FullName
    };

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<HelProjectCS> ProjectDependencies { get; init; } = ImmutableArray<HelProjectCS>.Empty;

    public ImmutableArray<HelPackageCS> PackageDependencies { get; init; } = ImmutableArray<HelPackageCS>.Empty;

    public HelAssemblyCS Assembly { get; init; } = HelAssemblyCS.Invalid;

    public HelSolutionReferenceCS ContainingSolution { get; init; } = HelSolutionReferenceCS.Invalid;
}
