
namespace Helveg.CSharp.Symbols;

public record FieldReference : SymbolReference
{
    public static FieldReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Field) };
}

public record FieldDefinition : MemberDefinition
{
    public static FieldDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Field) };

    public TypeReference FieldType { get; init; } = TypeReference.Invalid;

    public PropertyReference? AssociatedProperty { get; init; }

    public EventReference? AssociatedEvent { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsVolatile { get; init; }

    public bool IsConst { get; init; }

    public bool IsRequired { get; init; }

    public RefKind RefKind { get; init; }

    public FieldReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitField(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
