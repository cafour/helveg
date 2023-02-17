using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

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

    public HelReferenceCS ContainingSolution { get; init; } = HelReferenceCS.Invalid;
}
