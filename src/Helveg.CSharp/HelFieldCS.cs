namespace Helveg.CSharp;

public record HelFieldCS : HelSymbolBaseCS
{
    public static readonly HelFieldCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Field;

    public HelTypeCS Type { get; init; } = HelTypeCS.Invalid;

    public IHelSymbolCS? AssociatedSymbol { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsVolatile { get; init; }

    public bool IsConst { get; init; }

    public bool IsRequired { get; init; }

    public HelRefKindCS RefKind { get; init; }
}
