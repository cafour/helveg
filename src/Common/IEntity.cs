using System.Collections.Generic;
using System.Collections.Immutable;

namespace Helveg;

public interface IEntity
{
    string Id { get; }

    ImmutableArray<Diagnostic> Diagnostics { get; }

    ImmutableArray<IEntityExtension> Extensions { get; }

    IEntity AddExtension(IEntityExtension extension);

    IEntity AddExtensionRange(IEnumerable<IEntityExtension> extensions);

    void Accept(IEntityVisitor visitor);
}
