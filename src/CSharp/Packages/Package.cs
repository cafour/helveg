using System;
using System.Collections.Immutable;

namespace Helveg.CSharp.Packages;

public record Package : EntityBase
{
    public string? Version { get; init; }
    public string Name { get; init; } = Const.Invalid;

    public void Accept(IEntityVisitor visitor)
    {
    }
}
