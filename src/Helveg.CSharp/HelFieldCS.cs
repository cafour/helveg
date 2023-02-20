using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelFieldReferenceCS : HelReferenceCS, IInvalidable<HelFieldReferenceCS>
{
    public static HelFieldReferenceCS Invalid { get; } = new();
}

public record HelFieldCS : HelMemberCS, IInvalidable<HelFieldCS>
{
    public static HelFieldCS Invalid { get; } = new();

    public HelTypeReferenceCS FieldType { get; init; } = HelTypeReferenceCS.Invalid;

    public HelReferenceCS? AssociatedProperty { get; init; }

    public HelReferenceCS? AssociatedEvent { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsVolatile { get; init; }

    public bool IsConst { get; init; }

    public bool IsRequired { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public override HelFieldReferenceCS GetReference()
    {
        return new() { Token = Token };
    }
}
