using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record SolutionReference : EntityReference, IInvalidable<SolutionReference>
{
    public static SolutionReference Invalid { get; } = new();
}

public record SolutionDefinition : EntityDefinition, IInvalidable<SolutionDefinition>
{
    public static SolutionDefinition Invalid { get; } = new();

    /// <summary>
    /// The FullName of the solution file. Can be null if this is an automatically-generated solution.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<ProjectDefinition> Projects { get; init; } = ImmutableArray<ProjectDefinition>.Empty;

    public ImmutableArray<PackageDefinition> Packages { get; init; } = ImmutableArray<PackageDefinition>.Empty;

    public override SolutionReference GetReference()
    {
        return new() { Token = Token };
    }
}
