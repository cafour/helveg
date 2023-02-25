using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record ProjectReference : EntityReference, IInvalidable<ProjectReference>
{
    public static ProjectReference Invalid { get; } = new();
}

public record ProjectDefinition : EntityDefinition, IInvalidable<ProjectDefinition>
{
    public static ProjectDefinition Invalid { get; } = new();

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<ProjectReference> ProjectDependencies { get; init; }
        = ImmutableArray<ProjectReference>.Empty;

    public ImmutableArray<ProjectReference> PackageDependencies { get; init; }
        = ImmutableArray<ProjectReference>.Empty;

    public AssemblyDefinition Assembly { get; init; } = AssemblyDefinition.Invalid;

    public SolutionReference ContainingSolution { get; init; } = SolutionReference.Invalid;

    public override ProjectReference GetReference()
    {
        return new() { Token = Token };
    }
}
