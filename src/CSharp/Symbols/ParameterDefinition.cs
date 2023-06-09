
namespace Helveg.CSharp.Symbols;

public record ParameterReference : SymbolReference
{
    public static ParameterReference Invalid { get; } = new();
}

public record ParameterDefinition : SymbolDefinition
{
    public static ParameterDefinition Invalid { get; } = new();

    public TypeReference ParameterType { get; set; } = TypeReference.Invalid;

    public bool IsDiscard { get; init; }

    public RefKind RefKind { get; init; }

    public int Ordinal { get; init; }

    public bool IsParams { get; init; }

    public bool IsOptional { get; init; }

    public bool IsThis { get; init; }

    public bool HasExplicitDefaultValue { get; init; }

    public MethodReference? DeclaringMethod { get; init; }

    public PropertyReference? DeclaringProperty { get; init; }

    public ParameterReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitParameter(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
