using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelPropertyReferenceCS : HelReferenceCS, IInvalidable<HelPropertyReferenceCS>
{
    public static HelPropertyReferenceCS Invalid { get; } = new();
}

public record HelPropertyCS : HelMemberCS, IInvalidable<HelPropertyCS>
{
    public static HelPropertyCS Invalid { get; } = new();

    public HelTypeReferenceCS PropertyType { get; init; } = HelTypeReferenceCS.Invalid;

    public HelMethodReferenceCS? GetMethod { get; init; }

    public HelMethodReferenceCS? SetMethod { get; init; }

    // NOTE: https://github.com/dotnet/roslyn/pull/49659
    // public HelFieldReferenceCS? BackingField { get; init; }

    public bool IsIndexer { get; init; }

    public bool IsReadOnly => GetMethod is not null && SetMethod is null;

    public bool IsWriteOnly => GetMethod is null && SetMethod is not null;

    public bool IsRequired { get; init; }

    public HelRefKindCS RefKind { get; init; }

    public bool ReturnsByRef => RefKind == HelRefKindCS.Ref;

    public bool ReturnsByRefReadonly => RefKind == HelRefKindCS.RefReadOnly;

    public ImmutableArray<HelParameterCS> Parameters { get; init; } = ImmutableArray<HelParameterCS>.Empty;

    public HelPropertyReferenceCS? OverriddenProperty { get; init; }

    public override HelPropertyReferenceCS GetReference()
    {
        return new() { Token = Token, IsResolved = true };
    }
}
