
namespace Helveg.CSharp.Symbols;

public record EventReference : SymbolReference
{
    public static EventReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Event) };
}

public record EventDefinition : MemberDefinition
{
    public static EventDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Event) };

    public TypeReference EventType { get; init; } = TypeReference.Invalid;

    public MethodReference? AddMethod { get; init; }

    public MethodReference? RemoveMethod { get; init; }

    public MethodReference? RaiseMethod { get; init; }

    public EventReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
