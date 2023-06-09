using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp.Symbols;

public record NamespaceReference : SymbolReference
{
    public static NamespaceReference Invalid { get; } = new();
}

public record NamespaceDefinition : SymbolDefinition
{
    public static NamespaceDefinition Invalid { get; } = new();

    public ImmutableArray<TypeDefinition> Types { get; init; } = ImmutableArray<TypeDefinition>.Empty;

    public ImmutableArray<NamespaceDefinition> Namespaces { get; init; } = ImmutableArray<NamespaceDefinition>.Empty;

    public ModuleReference ContainingModule { get; init; } = ModuleReference.Invalid;

    public NamespaceReference? ContainingNamespace { get; init; }

    public bool IsGlobalNamespace => ContainingNamespace is null;

    public NamespaceReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public IEnumerable<TypeDefinition> GetAllTypes()
    {
        return Types
            .Concat(Types.SelectMany(t => t.NestedTypes))
            .Concat(Namespaces.SelectMany(n => n.GetAllTypes()));
    }

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitNamespace(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var subnamespace in Namespaces)
        {
            subnamespace.Accept(visitor);
        }

        foreach (var type in Types)
        {
            type.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
