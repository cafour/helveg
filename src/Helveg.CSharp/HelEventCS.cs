namespace Helveg.CSharp;

public record HelEventReferenceCS : HelMemberReferenceCS, IInvalidable<HelEventReferenceCS>
{
    public static HelEventReferenceCS Invalid { get; } = new();
}

public record HelEventCS : HelMemberCS<HelEventReferenceCS>, IInvalidable<HelEventCS>
{
    public static HelEventCS Invalid { get; } = new();

    public override HelEventReferenceCS Reference => new()
    {
        Name = Name,
        ContainingNamespace = ContainingNamespace,
        ContainingType = ContainingType
    };

    public HelTypeReferenceCS EventType { get; init; } = HelTypeReferenceCS.Invalid;

    public HelMethodReferenceCS? AddMethod { get; init; }

    public HelMethodReferenceCS? RemoveMethod { get; init; }

    public HelMethodReferenceCS? RaiseMethod { get; init; }
}
