using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public record PackageRepository : EntityBase
{
    public string? Name { get; init; }

    public ImmutableArray<Package> Packages { get; init; }
        = ImmutableArray<Package>.Empty;

    [JsonIgnore]
    public int Index { get; init; } = -1;

    [JsonIgnore]
    public NumericToken Token => NumericToken.Create(CSConst.CSharpNamespace, (int)RootKind.PackageRepository, Index);

    public override string Id
    {
        get => Token;
        init => Index = NumericToken.Parse(value, CSConst.CSharpNamespace, 3, (int)RootKind.PackageRepository).Values[^1];
    }

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
