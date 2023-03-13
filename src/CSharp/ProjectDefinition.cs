using System.Collections.Immutable;

namespace Helveg.CSharp;

public record ProjectReference : EntityReference
{
    public static ProjectReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Project) };
}

public record ProjectDefinition : EntityDefinition
{
    public static ProjectDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Project) };

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<ProjectReference> ProjectDependencies { get; init; }
        = ImmutableArray<ProjectReference>.Empty;

    public ImmutableArray<ProjectReference> PackageDependencies { get; init; }
        = ImmutableArray<ProjectReference>.Empty;

    public ImmutableArray<ExternalDependencyReference> ExternalDependencies { get; init; }
        = ImmutableArray<ExternalDependencyReference>.Empty;

    public AssemblyDefinition Assembly { get; init; } = AssemblyDefinition.Invalid;

    public SolutionReference ContainingSolution { get; init; } = SolutionReference.Invalid;

    public ProjectReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
