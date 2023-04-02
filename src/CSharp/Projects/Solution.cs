using System.Collections.Immutable;

namespace Helveg.CSharp.Projects;

public record SolutionDefinition : IEntity
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

    public SolutionReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
