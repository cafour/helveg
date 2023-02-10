using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelTypeReferenceCS : HelMemberReferenceCS, IInvalidable<HelTypeReferenceCS>
{
    public static HelTypeReferenceCS Invalid { get; } = new();

    public virtual HelTypeKindCS TypeKind { get; init; }

    public HelNullabilityCS Nullability { get; init; }

    public ImmutableArray<HelTypeParameterCS> TypeParameters { get; init; } = ImmutableArray<HelTypeParameterCS>.Empty;
}

public record HelTypeCS : HelMemberCS<HelTypeReferenceCS>, IInvalidable<HelTypeReferenceCS>
{
    public static HelTypeReferenceCS Invalid { get; } = new();

    public override HelTypeReferenceCS Reference => new()
    {
        Token = Token,
        Name = Name,
        ContainingNamespace = ContainingNamespace,
        ContainingType = ContainingType,
        TypeKind = TypeKind,
        TypeParameters = TypeParameters
    };

    public ImmutableArray<IHelSymbolCS> Members { get; init; } = ImmutableArray<IHelSymbolCS>.Empty;

    public ImmutableArray<HelTypeCS> NestedTypes { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public bool IsNested => ContainingType is not null;

    public HelTypeKindCS TypeKind { get; init; }

    public HelTypeReferenceCS? BaseType { get; init; }

    public ImmutableArray<HelTypeReferenceCS> Interfaces { get; init; } = ImmutableArray<HelTypeReferenceCS>.Empty;

    public ImmutableArray<HelTypeReferenceCS> AllInterfaces { get; init; } = ImmutableArray<HelTypeReferenceCS>.Empty;

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
