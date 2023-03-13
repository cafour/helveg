using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record NamespaceReference : EntityReference
{
    public static NamespaceReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Namespace) };
}

public record NamespaceDefinition : EntityDefinition
{
    public static NamespaceDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Namespace) };

    public ImmutableArray<TypeDefinition> Types { get; init; } = ImmutableArray<TypeDefinition>.Empty;

    public ImmutableArray<NamespaceDefinition> Namespaces { get; init; } = ImmutableArray<NamespaceDefinition>.Empty;

    public ModuleReference ContainingModule { get; init; } = ModuleReference.Invalid;

    public NamespaceReference? ContainingNamespace { get; init; }

    public bool IsGlobalNamespace => ContainingNamespace is null;

    public NamespaceReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return Types
            .Concat(Types.SelectMany(t => t.NestedTypes))
            .Concat(Namespaces.SelectMany(n => n.GetAllTypes()));
    }
}
