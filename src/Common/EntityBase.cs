using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg;

public record EntityBase : IEntity
{
    public virtual string Id { get; init; } = Const.Invalid;

    public ImmutableArray<Diagnostic> Diagnostics { get; init; }

    public ImmutableArray<IEntityExtension> Extensions { get; init; }

    public virtual void Accept(IEntityVisitor visitor)
    {
        foreach (var extension in Extensions)
        {
            if (extension is IVisitableEntityExtension visitable)
            {
                visitable.Accept(visitor);
            }
        }
    }
}
