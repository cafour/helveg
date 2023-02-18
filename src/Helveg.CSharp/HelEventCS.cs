using Helveg.Abstractions;

namespace Helveg.CSharp;

public record HelEventReferenceCS : HelReferenceCS, IInvalidable<HelEventReferenceCS>
{
    public static HelEventReferenceCS Invalid { get; } = new();
}

public record HelEventCS : HelMemberCS, IInvalidable<HelEventCS>
{
    public static HelEventCS Invalid { get; } = new();

    public HelTypeReferenceCS EventType { get; init; } = HelTypeReferenceCS.Invalid;

    public HelMethodReferenceCS? AddMethod { get; init; }

    public HelMethodReferenceCS? RemoveMethod { get; init; }

    public HelMethodReferenceCS? RaiseMethod { get; init; }

    public override HelEventReferenceCS GetReference()
    {
        return new() { Token = Token, IsResolved = true };
    }
}
