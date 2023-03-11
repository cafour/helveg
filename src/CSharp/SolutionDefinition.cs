using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record SolutionReference : EntityReference, IInvalidable<SolutionReference>
{
    public static SolutionReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Solution) };
}

public record SolutionDefinition : EntityDefinition, IInvalidable<SolutionDefinition>
{
    public static SolutionDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Solution) };

    /// <summary>
    /// The FullName of the solution file. Can be null if this is an automatically-generated solution.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<ProjectDefinition> Projects { get; init; } = ImmutableArray<ProjectDefinition>.Empty;

    public ImmutableArray<PackageDefinition> Packages { get; init; } = ImmutableArray<PackageDefinition>.Empty;

    public ImmutableArray<ExternalDependencyDefinition> ExternalDependencies { get; init; }
        = ImmutableArray<ExternalDependencyDefinition>.Empty;

    public override SolutionReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }
}
