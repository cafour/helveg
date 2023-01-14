namespace Helveg.CSharp;

public record HelPropertyCS : HelSymbolBaseCS
{
    public static readonly HelPropertyCS Invalid = new();
    
    public override HelSymbolKindCS Kind => HelSymbolKindCS.Property;

    public HelMethodCS? GetMethod { get; init; }

    public HelMethodCS? SetMethod { get; init; }
}
