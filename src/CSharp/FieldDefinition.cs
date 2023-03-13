
namespace Helveg.CSharp;

public record FieldReference : EntityReference
{
    public static FieldReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Field) };
}

public record FieldDefinition : MemberDefinition
{
    public static FieldDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Field) };

    public TypeReference FieldType { get; init; } = TypeReference.Invalid;

    public PropertyReference? AssociatedProperty { get; init; }

    public EventReference? AssociatedEvent { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsVolatile { get; init; }

    public bool IsConst { get; init; }

    public bool IsRequired { get; init; }

    public RefKind RefKind { get; init; }

    public FieldReference Reference => new() { Token = Token, Hint = Name };

    public override IEntityReference GetReference() => Reference;
}
