using Helveg.Abstractions;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record MethodReference : EntityReference, IInvalidable<MethodReference>
{
    public static MethodReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Method) };

    public ImmutableArray<TypeReference> TypeArguments { get; init; }
        = ImmutableArray<TypeReference>.Empty;
}

public record MethodDefinition : MemberDefinition, IInvalidable<MethodDefinition>
{
    public const string ConstructorName = ".ctor";
    public const string StaticConstructorName = ".cctor";

    public static MethodDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Method) };

    public MethodKind MethodKind { get; init; }

    public ImmutableArray<ParameterDefinition> Parameters { get; init; } = ImmutableArray<ParameterDefinition>.Empty;

    public ImmutableArray<TypeParameterDefinition> TypeParameters { get; init; } = ImmutableArray<TypeParameterDefinition>.Empty;

    public TypeReference? ReturnType { get; init; }

    public bool IsExtensionMethod { get; init; }

    public int Arity => TypeParameters.Length;
    
    public bool IsGenericMethod => TypeParameters.Length > 0;

    public bool IsAsync { get; init; }

    public bool ReturnsVoid => ReturnType is null;

    public bool ReturnsByRef => RefKind == RefKind.Ref;

    public bool ReturnsByRefReadonly => RefKind == RefKind.RefReadOnly;

    public RefKind RefKind { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsInitOnly { get; init; }

    public EntityReference? OverridenMethod { get; init; }

    public TypeReference? ReceiverType { get; init; }

    public ImmutableArray<MethodReference> ExplicitInterfaceImplementations { get; init; }

    public EntityReference? AssociatedEvent { get; init; }

    public EntityReference? AssociatedProperty { get; init; }

    public override MethodReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }
}
