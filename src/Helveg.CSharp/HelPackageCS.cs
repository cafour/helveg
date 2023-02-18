using Helveg.Abstractions;
using System;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelPackageReferenceCS : HelReferenceCS, IInvalidable<HelPackageReferenceCS>
{
    public static HelPackageReferenceCS Invalid { get; } = new();
}

public record HelPackageCS : HelDefinitionCS, IInvalidable<HelPackageCS>
{
    public static HelPackageCS Invalid { get; } = new();

    public string? Version { get; init; }

    public ImmutableArray<HelAssemblyCS> Assemblies { get; set; }

    public override HelPackageReferenceCS GetReference()
    {
        return new() { Token = Token, IsResolved = true };
    }
}
