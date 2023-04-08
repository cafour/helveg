
namespace Helveg.CSharp.Symbols;

public record TypeParameterReference : TypeReference
{
    public new static TypeParameterReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.TypeParameter) };

    public override TypeKind TypeKind => TypeKind.TypeParameter;
}

public record TypeParameterDefinition : SymbolDefinition
{
    public static TypeParameterDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.TypeParameter) };

    // TODO: Constraints

    public MethodReference? DeclaringMethod { get; init; }

    public TypeReference? DeclaringType { get; init; }

    public NamespaceReference ContainingNamespace { get; init; } = NamespaceReference.Invalid;

    public TypeParameterReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitTypeParameter(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
