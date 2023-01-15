namespace Helveg.CSharp;

public record HelEventCS : HelSymbolBaseCS
{
    public static readonly HelEventCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Event;

    public HelTypeCS Type { get; init; } = HelTypeCS.Invalid;

    public HelMethodCS? AddMethod { get; init; }

    public HelMethodCS? RemoveMethod { get; init; }

    public HelMethodCS? RaiseMethod { get; init; }
}
