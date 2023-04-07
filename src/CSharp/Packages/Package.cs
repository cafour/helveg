using System;
using System.Collections.Immutable;

namespace Helveg.CSharp.Packages;

public record Package : EntityBase
{
    public string? Version { get; init; }
    public string Name { get; init; } = Const.Invalid;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IPackageVisitor packageVisitor)
        {
            packageVisitor.VisitPackage(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
