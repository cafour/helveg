namespace Helveg.CSharp;

public record HelTypeParameterCS : HelSymbolBaseCS
{
    public static readonly HelTypeParameterCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.TypeParameter;

    public int Ordinal { get; init; }

    public HelMethodCS? ContainingMethod { get; init; }
}
