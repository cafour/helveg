using Helveg.Abstractions;

namespace Helveg.CSharp;

public record TypeParameterReference : TypeReference, IInvalidable<TypeParameterReference>
{
    public new static TypeParameterReference Invalid { get; } = new();

    public override TypeKind TypeKind => TypeKind.TypeParameter;
}

public record TypeParameterDefinition : TypeDefinition, IInvalidable<TypeParameterDefinition>
{
    public new static TypeParameterDefinition Invalid { get; } = new TypeParameterDefinition();

    // TODO: Constraints

    public MethodReference? DeclaringMethod { get; init; }

    public TypeReference? DeclaringType { get; init; }

    public override TypeParameterReference GetReference()
    {
        return new() { Token = Token };
    }
}
