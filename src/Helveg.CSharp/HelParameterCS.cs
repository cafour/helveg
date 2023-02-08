namespace Helveg.CSharp;

public record HelParameterReferenceCS : HelReferenceCS, IInvalidable<HelParameterReferenceCS>
{
    public static HelParameterReferenceCS Invalid { get; } = new();

    public int Ordinal { get; init; }

    public HelMethodReferenceCS Method { get; init; } = HelMethodReferenceCS.Invalid;
}

public record HelParameterCS : HelDefinitionCS<HelParameterReferenceCS>, IInvalidable<HelParameterCS>
{
    public static HelParameterCS Invalid { get; } = new();

    public override HelParameterReferenceCS Reference => new()
    {
        Name = Name,
        Ordinal = Ordinal,
        Method = Method
    };

    public HelTypeReferenceCS ParameterType { get; set; } = HelTypeReferenceCS.Invalid;

    public bool IsDiscard { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public int Ordinal { get; init; }

    public bool IsParams { get; init; }

    public bool IsOptional { get; init; }

    public bool IsThis { get; init; }

    public bool HasExplicitDefaultValue { get; init; }

    public HelMethodReferenceCS Method { get; init; } = HelMethodReferenceCS.Invalid;
}
