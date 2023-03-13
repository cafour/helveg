
namespace Helveg.CSharp;

public record ParameterReference : EntityReference
{
    public static ParameterReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Parameter) };
}

public record ParameterDefinition : EntityDefinition
{
    public static ParameterDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Parameter) };

    public TypeReference ParameterType { get; set; } = TypeReference.Invalid;

    public bool IsDiscard { get; init; }

    public RefKind RefKind { get; init; }

    public int Ordinal { get; init; }

    public bool IsParams { get; init; }

    public bool IsOptional { get; init; }

    public bool IsThis { get; init; }

    public bool HasExplicitDefaultValue { get; init; }

    public MethodReference? DeclaringMethod { get; init; }

    public PropertyReference? DeclaringProperty { get; init; }

    public ParameterReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
