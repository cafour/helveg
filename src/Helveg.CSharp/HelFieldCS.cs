using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelFieldReferenceCS : HelMemberReferenceCS, IInvalidable<HelFieldReferenceCS>
{
    public static HelFieldReferenceCS Invalid { get; } = new();
}

public record HelFieldCS : HelMemberCS<HelFieldReferenceCS>, IInvalidable<HelFieldCS>
{
    public static HelFieldCS Invalid { get; } = new();

    public override HelFieldReferenceCS Reference => new()
    {
        DefinitionToken = DefinitionToken,
        Name = Name
    };

    public HelTypeReferenceCS FieldType { get; init; } = HelTypeReferenceCS.Invalid;

    public HelPropertyReferenceCS? AssociatedProperty { get; init; }

    public HelEventReferenceCS? AssociatedEvent { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsVolatile { get; init; }

    public bool IsConst { get; init; }

    public bool IsRequired { get; init; }

    public HelRefKindCS RefKind { get; init; }
}
