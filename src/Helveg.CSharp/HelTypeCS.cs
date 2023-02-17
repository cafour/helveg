using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelTypeReferenceCS : HelMemberReferenceCS, IInvalidable<HelTypeReferenceCS>
{
    public static HelTypeReferenceCS Invalid { get; } = new();

    public virtual HelTypeKindCS TypeKind { get; init; }

    public HelNullabilityCS Nullability { get; init; }

    public int Arity { get; init; }
}

public record HelTypeCS : HelMemberCS<HelTypeReferenceCS>, IInvalidable<HelTypeCS>
{
    public static HelTypeCS Invalid { get; } = new();

    public override HelTypeReferenceCS Reference => new()
    {
        DefinitionToken = DefinitionToken,
        Name = Name,
        ContainingNamespace = ContainingNamespace,
        ContainingType = ContainingType,
        TypeKind = TypeKind,
        Arity = Arity
    };

    public ImmutableArray<HelPropertyCS> Properties { get; init; } = ImmutableArray<HelPropertyCS>.Empty;

    public ImmutableArray<HelFieldCS> Fields { get; init; } = ImmutableArray<HelFieldCS>.Empty;

    public ImmutableArray<HelEventCS> Events { get; init; } = ImmutableArray<HelEventCS>.Empty;

    public ImmutableArray<HelMethodCS> Methods { get; init; } = ImmutableArray<HelMethodCS>.Empty;

    public ImmutableArray<HelTypeCS> NestedTypes { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public bool IsNested => ContainingType is not null;

    public HelTypeKindCS TypeKind { get; init; }

    public HelTypeReferenceCS? BaseType { get; init; }

    public ImmutableArray<HelTypeReferenceCS> Interfaces { get; init; } = ImmutableArray<HelTypeReferenceCS>.Empty;

    public bool IsReferenceType { get; init; }

    public bool IsValueType { get; init; }

    public bool IsAnonymousType { get; init; }

    public bool IsTupleType { get; init; }

    public bool IsNativeIntegerType { get; init; }

    public bool IsRefLikeType { get; init; }

    public bool IsUnmanagedType { get; init; }

    public bool IsReadOnly { get; init; }

    public bool IsRecord { get; init; }

    public ImmutableArray<HelTypeParameterCS> TypeParameters { get; init; } = ImmutableArray<HelTypeParameterCS>.Empty;

    public int Arity => TypeParameters.Length;

    public bool IsGenericType => TypeParameters.Length > 0;

    public bool IsImplicitClass { get; init; }
}
