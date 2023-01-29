using System.Collections.Immutable;

namespace Helveg.CSharp;

// Merges ITypeSymbol, INamedTypeSymbol
public record HelTypeCS : HelSymbolBaseCS
{
    public static readonly HelTypeCS Invalid = new();

    public ImmutableArray<IHelSymbolCS> Members { get; init; } = ImmutableArray<IHelSymbolCS>.Empty;

    public ImmutableArray<HelTypeCS> NestedTypes { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public bool IsNested => ContainingType is not null;

    public HelTypeKindCS TypeKind { get; init; }

    public HelTypeCS? BaseType { get; init; }

    public ImmutableArray<HelTypeCS> Interfaces { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public ImmutableArray<HelTypeCS> AllInterfaces { get; init; } = ImmutableArray<HelTypeCS>.Empty;

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

    public bool IsUnboundGenericType { get; init; }

    public bool IsImplicitClass { get; init; }

    public ImmutableArray<HelTypeCS> TypeArguments { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public HelTypeCS? EnumUnderlyingType { get; init; }

    public HelTypeCS ConstructedFrom { get; init; } = HelTypeCS.Invalid;

    public IHelSymbolCS? AssociatedSymbol { get; init; }
}
