using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelTypeParameterReferenceCS : HelTypeReferenceCS, IInvalidable<HelTypeParameterReferenceCS>
{
    public new static HelTypeParameterReferenceCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.TypeParameter;

    public int Ordinal { get; init; }

    public HelMethodReferenceCS? DeclaringMethod { get; init; }

    public HelTypeReferenceCS? DeclaringType { get; init; }
}

public record HelImmediateTypeReferenceCS : HelTypeReferenceCS, IInvalidable<HelImmediateTypeReferenceCS>
{
    public new static HelImmediateTypeReferenceCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.TypeParameter;

    public int Ordinal { get; init; }
}

public record HelTypeParameterCS : HelTypeCS, IInvalidable<HelTypeParameterCS>
{
    public new static HelTypeParameterCS Invalid { get; } = new HelTypeParameterCS();

    // TODO: Constraints

    public override HelTypeReferenceCS Reference => new HelTypeParameterReferenceCS
    {
        Token = Token,
        Name = Name,
        ContainingNamespace = ContainingNamespace,
        ContainingType = ContainingType,
        DeclaringMethod = DeclaringMethod,
        DeclaringType = DeclaringType
    };

    public HelMethodReferenceCS? DeclaringMethod { get; init; }

    public HelTypeReferenceCS? DeclaringType { get; init; }
}
