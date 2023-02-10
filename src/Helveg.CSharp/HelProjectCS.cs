using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelProjectReferenceCS : HelReferenceCS, IInvalidable<HelProjectReferenceCS>
{
    public static HelProjectReferenceCS Invalid { get; } = new();

    public string? FullName { get; init; }
}

public record HelProjectCS : HelDefinitionCS<HelProjectReferenceCS>, IInvalidable<HelProjectCS>
{
    public static HelProjectCS Invalid { get; } = new();

    public override HelProjectReferenceCS Reference => new()
    {
        Token = Token,
        Name = Name,
        FullName = FullName
    };

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<HelProjectReferenceCS> ProjectDependencies { get; init; }
        = ImmutableArray<HelProjectReferenceCS>.Empty;

    public ImmutableArray<HelPackageReferenceCS> PackageDependencies { get; init; }
        = ImmutableArray<HelPackageReferenceCS>.Empty;

    public HelAssemblyCS Assembly { get; init; } = HelAssemblyCS.Invalid;

    public HelSolutionReferenceCS ContainingSolution { get; init; } = HelSolutionReferenceCS.Invalid;
}
