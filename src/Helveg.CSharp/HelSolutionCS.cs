using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelSolutionReferenceCS : HelReferenceCS, IDefaultInvalid<HelSolutionReferenceCS>
{
    public string? FullName { get; set; }
}

public record HelSolutionCS : HelDefinitionCS, IDefaultInvalid<HelSolutionCS>
{
    /// <summary>
    /// The FullName of the solution file. Can be null if this is an automatically-generated solution.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<HelProjectCS> Projects { get; init; } = ImmutableArray<HelProjectCS>.Empty;
}
