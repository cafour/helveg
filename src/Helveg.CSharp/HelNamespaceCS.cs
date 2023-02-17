using Helveg.Abstractions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelNamespaceReferenceCS : HelReferenceCS, IInvalidable<HelNamespaceReferenceCS>
{
    public static HelNamespaceReferenceCS Invalid { get; } = new();

    public HelNamespaceReferenceCS? ContainingNamespace { get; init; }

    public HelModuleReferenceCS ContainingModule { get; init; } = HelModuleReferenceCS.Invalid;
}

public record HelNamespaceCS : HelDefinitionCS<HelNamespaceReferenceCS>, IInvalidable<HelNamespaceCS>
{
    public static HelNamespaceCS Invalid { get; } = new();

    public override HelNamespaceReferenceCS Reference => new()
    {
        Token = Token,
        Name = Name,
        ContainingModule = ContainingModule,
        ContainingNamespace = ContainingNamespace
    };

    public ImmutableArray<HelTypeCS> Types { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public ImmutableArray<HelNamespaceCS> Namespaces { get; init; } = ImmutableArray<HelNamespaceCS>.Empty;

    public HelModuleReferenceCS ContainingModule { get; init; } = HelModuleReferenceCS.Invalid;

    public HelNamespaceReferenceCS? ContainingNamespace { get; init; }

    public bool IsGlobalNamespace => ContainingNamespace is null;
}
