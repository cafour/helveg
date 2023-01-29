using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelPropertyCS : HelSymbolBaseCS
{
    public static readonly HelPropertyCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Property;

    public HelTypeCS Type { get; init; } = HelTypeCS.Invalid;

    public HelMethodCS? GetMethod { get; init; }

    public HelMethodCS? SetMethod { get; init; }

    public bool IsIndex { get; init; }

    public bool IsReadOnly => GetMethod is not null && SetMethod is null;

    public bool IsWriteOnly => GetMethod is null && SetMethod is not null;

    public bool IsRequired { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public bool ReturnsByRef => RefKind == HelRefKindCS.Ref;

    public bool ReturnsByRefReadonly => RefKind == HelRefKindCS.RefReadOnly;

    public ImmutableArray<HelParameterCS> Parameters { get; init; } = ImmutableArray<HelParameterCS>.Empty;

    public HelPropertyCS? OverriddenProperty { get; init; }
}
