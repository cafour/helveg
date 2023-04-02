
namespace Helveg.CSharp.Symbols;

public record TypeParameterReference : TypeReference
{
    public new static TypeParameterReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.TypeParameter) };

    public override TypeKind TypeKind => TypeKind.TypeParameter;
}

public record TypeParameterDefinition : TypeDefinition
{
    public new static TypeParameterDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.TypeParameter) };

    // TODO: Constraints

    public MethodReference? DeclaringMethod { get; init; }

    public TypeReference? DeclaringType { get; init; }

    public new TypeParameterReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
