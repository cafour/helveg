using System.Collections.Immutable;

namespace Helveg.CSharp.Projects;

public record Project : EntityBase
{
    public static Project Invalid { get; } = new();

    /// <summary>
    /// The FullName of the project file.
    /// </summary>
    public string? FullName { get; init; }

    public string ContainingSolution { get; init; } = Const.Invalid;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IProjectVisitor projectVisitor)
        {
            projectVisitor.VisitProject(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
