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
        = ImmutableArray<Diagnostic>.Empty;
        
    public ImmutableArray<Comment> Comments { get; init; }
        = ImmutableArray<Comment>.Empty;

    public ImmutableArray<IEntityExtension> Extensions { get; init; }
        = ImmutableArray<IEntityExtension>.Empty;

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

    IEntity IEntity.AddExtension(IEntityExtension extension)
    {
        return this with
        {
            Extensions = Extensions.Add(extension)
        };
    }

    IEntity IEntity.AddExtensionRange(IEnumerable<IEntityExtension> extensions)
    {
        return this with
        {
            Extensions = Extensions.AddRange(extensions)
        };
    }
}
