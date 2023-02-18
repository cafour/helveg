using Helveg.Abstractions;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelMethodReferenceCS : HelReferenceCS, IInvalidable<HelMethodReferenceCS>
{
    public static HelMethodReferenceCS Invalid { get; } = new();

    public ImmutableArray<HelTypeReferenceCS> TypeArguments { get; init; }
        = ImmutableArray<HelTypeReferenceCS>.Empty;
}

public record HelMethodCS : HelMemberCS, IInvalidable<HelMethodCS>
{
    public const string ConstructorName = ".ctor";
    public const string StaticConstructorName = ".cctor";

    public static HelMethodCS Invalid { get; } = new();

    public HelMethodKindCS MethodKind { get; init; }

    public ImmutableArray<HelParameterCS> Parameters { get; init; } = ImmutableArray<HelParameterCS>.Empty;

    public ImmutableArray<HelTypeParameterCS> TypeParameters { get; init; } = ImmutableArray<HelTypeParameterCS>.Empty;

    public HelTypeReferenceCS? ReturnType { get; init; }

    public bool IsExtensionMethod { get; init; }

    public int Arity => TypeParameters.Length;
    
    public bool IsGenericMethod => TypeParameters.Length > 0;

    public bool IsAsync { get; init; }

    public bool ReturnsVoid => ReturnType is null;

    public bool ReturnsByRef => RefKind == HelRefKindCS.Ref;

    public bool ReturnsByRefReadonly => RefKind == HelRefKindCS.RefReadOnly;

    public HelRefKindCS RefKind { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsInitOnly { get; init; }

    public HelReferenceCS? OverridenMethod { get; init; }

    public HelTypeReferenceCS? ReceiverType { get; init; }

    public ImmutableArray<HelMethodReferenceCS> ExplicitInterfaceImplementations { get; init; }

    public HelReferenceCS? AssociatedEvent { get; init; }

    public HelReferenceCS? AssociatedProperty { get; init; }

    public override HelMethodReferenceCS GetReference()
    {
        return new() { Token = Token };
    }
}
