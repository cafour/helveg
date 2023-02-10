using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelPropertyReferenceCS : HelMemberReferenceCS, IInvalidable<HelPropertyReferenceCS>
{
    public static HelPropertyReferenceCS Invalid { get; } = new();
}

public record HelPropertyCS : HelMemberCS<HelPropertyReferenceCS>, IInvalidable<HelPropertyCS>
{
    public static HelPropertyCS Invalid { get; } = new();

    public override HelPropertyReferenceCS Reference => new()
    {
        Token = Token,
        Name = Name,
        ContainingNamespace = ContainingNamespace,
        ContainingType = ContainingType
    };

    public HelTypeReferenceCS PropertyType { get; init; } = HelTypeReferenceCS.Invalid;

    public HelMethodReferenceCS? GetMethod { get; init; }

    public HelMethodReferenceCS? SetMethod { get; init; }

    public bool IsIndex { get; init; }

    public bool IsReadOnly => GetMethod is not null && SetMethod is null;

    public bool IsWriteOnly => GetMethod is null && SetMethod is not null;

    public bool IsRequired { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public bool ReturnsByRef => RefKind == HelRefKindCS.Ref;

    public bool ReturnsByRefReadonly => RefKind == HelRefKindCS.RefReadOnly;

    public ImmutableArray<HelParameterCS> Parameters { get; init; } = ImmutableArray<HelParameterCS>.Empty;

    public HelPropertyReferenceCS? OverriddenProperty { get; init; }
}
