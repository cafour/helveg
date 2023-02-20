using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelSolutionReferenceCS : HelReferenceCS, IInvalidable<HelSolutionReferenceCS>
{
    public static HelSolutionReferenceCS Invalid { get; }
}

public record HelSolutionCS : HelDefinitionCS, IInvalidable<HelSolutionCS>
{
    public static HelSolutionCS Invalid { get; } = new();

    /// <summary>
    /// The FullName of the solution file. Can be null if this is an automatically-generated solution.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<HelProjectCS> Projects { get; init; } = ImmutableArray<HelProjectCS>.Empty;

    public ImmutableArray<HelPackageCS> Packages { get; init; } = ImmutableArray<HelPackageCS>.Empty;

    public override HelSolutionReferenceCS GetReference()
    {
        return new() { Token = Token };
    }
}
