
namespace Helveg.CSharp.Symbols;

public record EventReference : SymbolReference
{
    public static EventReference Invalid { get; } = new();
}

public record EventDefinition : MemberDefinition
{
    public static EventDefinition Invalid { get; } = new();

    public TypeReference EventType { get; init; } = TypeReference.Invalid;

    public MethodReference? AddMethod { get; init; }

    public MethodReference? RemoveMethod { get; init; }

    public MethodReference? RaiseMethod { get; init; }

    public EventReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitEvent(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        base.Accept(visitor);
    }
}
