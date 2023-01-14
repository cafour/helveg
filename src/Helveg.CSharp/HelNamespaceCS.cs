using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelNamespaceCS : IHelEntityCS
{
    public static readonly HelNamespaceCS Invalid = new();

    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public ImmutableArray<HelTypeCS> Types { get; init; } = ImmutableArray.Create<HelTypeCS>();

    public HelNamespaceCS? ContainingNamespace { get; init; }

    [JsonIgnore]
    public bool IsGlobal => ContainingNamespace is null;

    public IEnumerable<IHelSymbolCS> GetAllSymbols()
    {
        return Types.SelectMany(t => t.Members);
    }
}
