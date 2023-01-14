namespace Helveg.CSharp;

public record HelEventCS : HelSymbolBaseCS
{
    public static readonly HelEventCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Event;

    public HelMethodCS? AddMethod { get; init; }

    public HelMethodCS? RemoveMethod { get; init; }
}
