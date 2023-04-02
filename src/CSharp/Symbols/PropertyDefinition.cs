using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record PropertyReference : SymbolReference
{
    public static PropertyReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Property) };
}

public record PropertyDefinition : MemberDefinition
{
    public static PropertyDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Property) };

    public TypeReference PropertyType { get; init; } = TypeReference.Invalid;

    public MethodReference? GetMethod { get; init; }

    public MethodReference? SetMethod { get; init; }

    // NOTE: https://github.com/dotnet/roslyn/pull/49659
    // public HelFieldReferenceCS? BackingField { get; init; }

    public bool IsIndexer { get; init; }

    public bool IsReadOnly => GetMethod is not null && SetMethod is null;

    public bool IsWriteOnly => GetMethod is null && SetMethod is not null;

    public bool IsRequired { get; init; }

    public RefKind RefKind { get; init; }

    public bool ReturnsByRef => RefKind == RefKind.Ref;

    public bool ReturnsByRefReadonly => RefKind == RefKind.RefReadOnly;

    public ImmutableArray<ParameterDefinition> Parameters { get; init; } = ImmutableArray<ParameterDefinition>.Empty;

    public PropertyReference? OverriddenProperty { get; init; }

    public PropertyReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
