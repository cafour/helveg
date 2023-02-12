using Helveg.Abstractions;
using System;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelPackageReferenceCS : HelReferenceCS, IInvalidable<HelPackageReferenceCS>
{
    public static HelPackageReferenceCS Invalid { get; } = new();

    public string? Version { get; init; }
}

public record HelPackageCS : HelDefinitionCS<HelPackageReferenceCS>, IInvalidable<HelPackageCS>
{
    public static HelPackageCS Invalid { get; } = new();

    public override HelPackageReferenceCS Reference => new()
    {
        DefinitionToken = DefinitionToken,
        Name = Name,
        Version = Version
    };

    public string? Version { get; init; }

    public ImmutableArray<HelAssemblyCS> Assemblies { get; set; }
}
