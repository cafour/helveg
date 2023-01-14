namespace Helveg.CSharp;

public record HelParameterCS : HelSymbolBaseCS
{
    public static readonly HelParameterCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Parameter;

    public HelTypeCS Type { get; set; } = HelTypeCS.Invalid;

    public bool IsDiscard { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public int Ordinal { get; init; }
}
