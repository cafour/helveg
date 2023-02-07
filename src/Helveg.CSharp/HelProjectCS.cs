using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelProjectReferenceCS : HelReferenceCS, IDefaultInvalid<HelProjectReferenceCS>
{
    public string? FullName { get; init; }

    public HelSolutionReferenceCS ContainingSolution { get; init; } = IDefaultInvalid<HelSolutionReferenceCS>.Invalid;
}

public record HelProjectCS : HelDefinitionCS<HelProjectReferenceCS>, IDefaultInvalid<HelProjectCS>
{
    public static readonly HelProjectCS Invalid = new();

    public override HelProjectReferenceCS Reference => new() { Name = Name };

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<HelProjectCS> ProjectDependencies { get; init; } = ImmutableArray.Create<HelProjectCS>();

    public ImmutableArray<HelPackageCS> PackageDependencies { get; init; } = ImmutableArray.Create<HelPackageCS>();

    public HelAssemblyCS Assembly { get; init; } = HelAssemblyCS.Invalid;

    public HelSolutionReferenceCS ContainingSolution { get; init; } = IDefaultInvalid<HelSolutionReferenceCS>.Invalid;
}
