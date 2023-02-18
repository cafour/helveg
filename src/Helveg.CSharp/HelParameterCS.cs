using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelParameterReferenceCS : HelReferenceCS, IInvalidable<HelParameterReferenceCS>
{
    public static HelParameterReferenceCS Invalid { get; } = new();
}

public record HelParameterCS : HelDefinitionCS, IInvalidable<HelParameterCS>
{
    public static HelParameterCS Invalid { get; } = new();

    public HelTypeReferenceCS ParameterType { get; set; } = HelTypeReferenceCS.Invalid;

    public bool IsDiscard { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public int Ordinal { get; init; }

    public bool IsParams { get; init; }

    public bool IsOptional { get; init; }

    public bool IsThis { get; init; }

    public bool HasExplicitDefaultValue { get; init; }

    public HelMethodReferenceCS? DeclaringMethod { get; init; }

    public HelPropertyReferenceCS? DeclaringProperty { get; init; }

    public override HelParameterReferenceCS GetReference()
    {
        return new() { Token = Token, IsResolved = true };
    }
}
