using Helveg.Abstractions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelNamespaceCS : HelDefinitionCS, IInvalidable<HelNamespaceCS>
{
    public static HelNamespaceCS Invalid { get; } = new();

    public ImmutableArray<HelTypeCS> Types { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public ImmutableArray<HelNamespaceCS> Namespaces { get; init; } = ImmutableArray<HelNamespaceCS>.Empty;

    public HelReferenceCS ContainingModule { get; init; } = HelModuleReferenceCS.Invalid;

    public HelReferenceCS? ContainingNamespace { get; init; }

    public bool IsGlobalNamespace => ContainingNamespace is null;
}
