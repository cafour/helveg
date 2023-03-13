
namespace Helveg.CSharp;

public record EventReference : EntityReference
{
    public static EventReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Event) };
}

public record EventDefinition : MemberDefinition
{
    public static EventDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Event) };

    public TypeReference EventType { get; init; } = TypeReference.Invalid;

    public MethodReference? AddMethod { get; init; }

    public MethodReference? RemoveMethod { get; init; }

    public MethodReference? RaiseMethod { get; init; }

    public override EventReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }
}
