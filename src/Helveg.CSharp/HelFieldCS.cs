namespace Helveg.CSharp;

public record HelFieldCS : HelSymbolBaseCS
{
    public static readonly HelFieldCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Field;

    public HelTypeCS Type { get; init; } = HelTypeCS.Invalid;
}
