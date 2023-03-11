using Helveg.Abstractions;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record PropertyReference : EntityReference, IInvalidable<PropertyReference>
{
    public static PropertyReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Property) };
}

public record PropertyDefinition : MemberDefinition, IInvalidable<PropertyDefinition>
{
    public static PropertyDefinition Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Property) };

    public TypeReference PropertyType { get; init; } = TypeReference.Invalid;

    public MethodReference? GetMethod { get; init; }

    public MethodReference? SetMethod { get; init; }

    // NOTE: https://github.com/dotnet/roslyn/pull/49659
    // public HelFieldReferenceCS? BackingField { get; init; }

    public bool IsIndexer { get; init; }

    public bool IsReadOnly => GetMethod is not null && SetMethod is null;

    public bool IsWriteOnly => GetMethod is null && SetMethod is not null;

    public bool IsRequired { get; init; }

    public RefKind RefKind { get; init; }

    public bool ReturnsByRef => RefKind == RefKind.Ref;

    public bool ReturnsByRefReadonly => RefKind == RefKind.RefReadOnly;

    public ImmutableArray<ParameterDefinition> Parameters { get; init; } = ImmutableArray<ParameterDefinition>.Empty;

    public PropertyReference? OverriddenProperty { get; init; }

    public override PropertyReference GetReference()
    {
        return new() { Token = Token, Hint = Name };
    }
}
