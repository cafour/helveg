using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp.Symbols;

public record MethodReference : SymbolReference
{
    public static MethodReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Method) };

    public ImmutableArray<TypeReference> TypeArguments { get; init; }
        = ImmutableArray<TypeReference>.Empty;
}

public record MethodDefinition : MemberDefinition
{
    public const string ConstructorName = ".ctor";
    public const string StaticConstructorName = ".cctor";

    public static MethodDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Method) };

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

    public SymbolReference? OverridenMethod { get; init; }

    public TypeReference? ReceiverType { get; init; }

    public ImmutableArray<MethodReference> ExplicitInterfaceImplementations { get; init; }

    public SymbolReference? AssociatedEvent { get; init; }

    public SymbolReference? AssociatedProperty { get; init; }

    public MethodReference Reference => new() { Token = Token, Hint = Name };

    public override ISymbolReference GetReference() => Reference;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitMethod(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var typeParameter in TypeParameters)
        {
            typeParameter.Accept(visitor);
        }

        foreach (var parameter in Parameters)
        {
            parameter.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
