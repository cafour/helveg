using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelTypeCS : HelSymbolBaseCS
{
    public static readonly HelTypeCS Invalid = new();

    public ImmutableArray<IHelSymbolCS> Members { get; init; } = ImmutableArray.Create<IHelSymbolCS>();

    public bool IsSealed { get; init; }

    [JsonIgnore]
    public ImmutableArray<HelTypeCS> NestedTypes { get; init; } = ImmutableArray.Create<HelTypeCS>();

    [JsonIgnore]
    public bool IsNested => ContainingType is not null;
}
