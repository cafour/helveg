using System.Collections.Immutable;

namespace Helveg;

public interface IEntity
{
    string Id { get; }

    ImmutableArray<Diagnostic> Diagnostics { get; }

    ImmutableArray<IEntityExtension> Extensions { get; }

    void Accept(IEntityVisitor visitor);
}
