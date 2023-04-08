using System.Collections.Immutable;

namespace Helveg.CSharp.Projects;

public record Solution : EntityBase
{
    public static Solution Invalid { get; } = new();

    public string? Path { get; init; }

    public string Name { get; init; } = Const.Invalid;

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
