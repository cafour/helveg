using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelMethodCS : HelSymbolBaseCS
{
    public const string ConstructorName = ".ctor";
    public const string StaticConstructorName = ".cctor";

    public static readonly HelMethodCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Method;

    public HelMethodKindCS MethodKind { get; init; }

    public ImmutableArray<HelParameterCS> Parameters { get; init; } = ImmutableArray<HelParameterCS>.Empty;

    public ImmutableArray<HelTypeParameterCS> TypeParameters { get; init; } = ImmutableArray<HelTypeParameterCS>.Empty;

    public ImmutableArray<HelTypeCS> TypeArguments { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public HelTypeCS? ReturnType { get; init; }

    public bool IsExtensionMethod { get; init; }

    public int Arity => TypeParameters.Length;
    
    public bool IsGenericMethod => TypeParameters.Length > 0;

    public bool IsAsync { get; init; }

    public bool ReturnsVoid => ReturnType is null;

    public bool ReturnsByRef => RefKind == HelRefKindCS.Ref;

    public bool ReturnsByRefReadonly => RefKind == HelRefKindCS.RefReadOnly;

    public HelRefKindCS RefKind { get; init; }

    public HelMethodCS ConstructedFrom { get; init; } = HelMethodCS.Invalid;

    public bool IsReadOnly { get; init; }

    public bool IsInitOnly { get; init; }

    public HelMethodCS? OverridenMethod { get; init; }

    public HelTypeCS? ReceiverType { get; init; }

    public ImmutableArray<HelMethodCS> ExplicitInterfaceImplementations { get; init; }

    public IHelSymbolCS? AssociatedSymbol { get; init; }
}
