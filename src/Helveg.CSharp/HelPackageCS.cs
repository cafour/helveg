using Helveg.Abstractions;
using System;
using System.Collections.Immutable;

namespace Helveg.CSharp;

public record HelPackageCS : HelDefinitionCS, IInvalidable<HelPackageCS>
{
    public static HelPackageCS Invalid { get; } = new();

    public string? Version { get; init; }

    public ImmutableArray<HelAssemblyCS> Assemblies { get; set; }
}
