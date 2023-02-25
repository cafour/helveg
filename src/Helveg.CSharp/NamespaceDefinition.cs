using Helveg.Abstractions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record NamespaceReference : EntityReference, IInvalidable<NamespaceReference>
{
    public static NamespaceReference Invalid { get; } = new();
}

public record NamespaceDefinition : EntityDefinition, IInvalidable<NamespaceDefinition>
{
    public static NamespaceDefinition Invalid { get; } = new();

    public ImmutableArray<TypeDefinition> Types { get; init; } = ImmutableArray<TypeDefinition>.Empty;

    public ImmutableArray<NamespaceDefinition> Namespaces { get; init; } = ImmutableArray<NamespaceDefinition>.Empty;

    public ModuleReference ContainingModule { get; init; } = ModuleReference.Invalid;

    public NamespaceReference? ContainingNamespace { get; init; }

    public bool IsGlobalNamespace => ContainingNamespace is null;

    public override NamespaceReference GetReference()
    {
        return new() { Token = Token };
    }
}
