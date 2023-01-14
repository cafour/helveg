using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelProjectCS : IHelEntityCS
{
    public static readonly HelProjectCS Invalid = new();

    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public ImmutableArray<HelNamespaceCS> Namespaces { get; init; } = ImmutableArray.Create<HelNamespaceCS>();

    [JsonIgnore]
    public ImmutableArray<HelProjectCS> ProjectDependencies { get; init; } = ImmutableArray.Create<HelProjectCS>();

    [JsonIgnore]
    public ImmutableArray<HelPackageCS> PackageDependencies { get; init; } = ImmutableArray.Create<HelPackageCS>();

    public IEnumerable<IHelSymbolCS> GetAllSymbols()
    {
        return Namespaces.SelectMany(n => n.GetAllSymbols());
    }
}
