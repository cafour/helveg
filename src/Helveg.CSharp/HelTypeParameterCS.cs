using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelTypeParameterReferenceCS : HelTypeReferenceCS, IInvalidable<HelTypeParameterReferenceCS>
{
    public new static HelTypeParameterReferenceCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.TypeParameter;
}

public record HelTypeParameterCS : HelTypeCS, IInvalidable<HelTypeParameterCS>
{
    public new static HelTypeParameterCS Invalid { get; } = new HelTypeParameterCS();

    // TODO: Constraints

    public HelMethodReferenceCS? DeclaringMethod { get; init; }

    public HelTypeReferenceCS? DeclaringType { get; init; }

    public override HelTypeParameterReferenceCS GetReference()
    {
        return new() { Token = Token };
    }
}
