using Helveg.Abstractions;

namespace Helveg.CSharp;

public record EventReference : EntityReference, IInvalidable<EventReference>
{
    public static EventReference Invalid { get; } = new();
}

public record EventDefinition : MemberDefinition, IInvalidable<EventDefinition>
{
    public static EventDefinition Invalid { get; } = new();

    public TypeReference EventType { get; init; } = TypeReference.Invalid;

    public MethodReference? AddMethod { get; init; }

    public MethodReference? RemoveMethod { get; init; }

    public MethodReference? RaiseMethod { get; init; }

    public override EventReference GetReference()
    {
        return new() { Token = Token };
    }
}
