using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.CSharp.Packages;

public record Package : EntityBase
{
    [JsonIgnore]
    public NumericToken Token { get; init; }

    public override string Id { get => Token; init => Token = NumericToken.Parse(value); }

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
