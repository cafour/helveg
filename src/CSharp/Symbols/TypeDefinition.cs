using System.Collections.Immutable;

namespace Helveg.CSharp.Symbols;

public record TypeReference : SymbolReference
{
    public static TypeReference Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Type) };

    public virtual TypeKind TypeKind { get; init; }

    public TypeNullability Nullability { get; init; }

    public ImmutableArray<TypeReference> TypeArguments { get; init; }
        = ImmutableArray<TypeReference>.Empty;
}

public record TypeDefinition : MemberDefinition
{
    public static TypeDefinition Invalid { get; }
        = new() { Token = SymbolToken.CreateError(SymbolKind.Type) };

    public string MetadataName { get; init; } = Const.Invalid;

    public ImmutableArray<PropertyDefinition> Properties { get; init; }
        = ImmutableArray<PropertyDefinition>.Empty;

    public ImmutableArray<FieldDefinition> Fields { get; init; }
        = ImmutableArray<FieldDefinition>.Empty;

    public ImmutableArray<EventDefinition> Events { get; init; }
        = ImmutableArray<EventDefinition>.Empty;

    public ImmutableArray<MethodDefinition> Methods { get; init; }
        = ImmutableArray<MethodDefinition>.Empty;

    public ImmutableArray<TypeDefinition> NestedTypes { get; init; }
        = ImmutableArray<TypeDefinition>.Empty;

    public bool IsNested => ContainingType is not null;

    public TypeKind TypeKind { get; init; }

    public TypeReference? BaseType { get; init; }

    public ImmutableArray<TypeReference> Interfaces { get; init; } = ImmutableArray<TypeReference>.Empty;

    public bool IsReferenceType { get; init; }

    public bool IsValueType { get; init; }

    public bool IsAnonymousType { get; init; }

    public bool IsTupleType { get; init; }

    public bool IsNativeIntegerType { get; init; }

    public bool IsRefLikeType { get; init; }

    public bool IsUnmanagedType { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsRecord { get; init; }

    public ImmutableArray<TypeParameterDefinition> TypeParameters { get; init; }
        = ImmutableArray<TypeParameterDefinition>.Empty;

    public int Arity => TypeParameters.Length;

    public bool IsGenericType => TypeParameters.Length > 0;

    public bool IsImplicitClass { get; init; }

    public TypeReference Reference => new() { Token = Token, TypeKind = TypeKind, Hint = Name };

    public override IEntityReference GetReference() => Reference;

    public override void Accept(IEntityVisitor visitor)
    {
        if (visitor is ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.VisitType(this);
        }
        else
        {
            visitor.DefaultVisit(this);
        }

        foreach (var typeParameter in TypeParameters)
        {
            typeParameter.Accept(visitor);
        }

        foreach (var nestedType in NestedTypes)
        {
            nestedType.Accept(visitor);
        }

        foreach (var field in Fields)
        {
            field.Accept(visitor);
        }

        foreach (var @event in Events)
        {
            @event.Accept(visitor);
        }

        foreach (var property in Properties)
        {
            property.Accept(visitor);
        }

        foreach (var method in Methods)
        {
            method.Accept(visitor);
        }

        base.Accept(visitor);
    }
}
