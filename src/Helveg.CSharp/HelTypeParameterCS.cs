using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelTypeParameterCS : HelTypeReferenceCS, IInvalidable<HelTypeParameterCS>
{
    public new static HelTypeParameterCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.TypeParameter;

    public int Ordinal { get; init; }

    public HelMethodReferenceCS? DeclaringMethod { get; init; }

    public HelTypeReferenceCS? DeclaringType { get; init; }
}
