using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public record PackageRepository : EntityBase
{
    public string? Name { get; init; }

    public ImmutableArray<Package> Packages { get; init; }
        = new ImmutableArray<Package>();

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is IPackageVisitor packageVisitor)
        {
            packageVisitor.VisitPackageRepository(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var package in Packages)
        {
            package.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
