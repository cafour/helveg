using System.Collections.Immutable;

namespace Helveg.CSharp.Projects;

public record Solution : EntityBase
{
    public static Solution Invalid { get; } = new();

    /// <summary>
    /// The FullName of the solution file. Can be null if this is an automatically-generated solution.
    /// </summary>
    public string? FullName { get; init; }

    public ImmutableArray<Project> Projects { get; init; } = ImmutableArray<Project>.Empty;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitSolution(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach(var project in Projects)
        {
            project.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
