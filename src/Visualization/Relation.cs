using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record Relation
{
    [JsonIgnore]
    public string Id { get; init; } = Const.Invalid;

    public ImmutableDictionary<string, Edge> Edges { get; init; }
        = ImmutableDictionary<string, Edge>.Empty;

    public Relation(string id, ImmutableArray<Edge> edges = default)
    {
        Id = id;
        Edges = edges.IsDefault
            ? ImmutableDictionary<string, Edge>.Empty
            : edges.ToImmutableDictionary(x => $"{Id};{x.Src};{x.Dst}");
    }

    public Relation()
    {
    }
}
