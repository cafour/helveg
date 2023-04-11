using System;
using System.Collections.Immutable;

namespace Helveg.CSharp.Packages;

public record Package : EntityBase
{
    public string Name { get; init; } = Const.Invalid;

    public ImmutableArray<string> Versions { get; init; }
        = ImmutableArray<string>.Empty;

    public string? Url { get; init; }

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

public record PackageReference
{
    public string Id { get; init; } = Const.Invalid;

    public string? Version { get; init; }
}
