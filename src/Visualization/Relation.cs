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

    public ImmutableArray<Edge> Edges { get; init; }
        = ImmutableArray<Edge>.Empty;

    public Relation(string id, ImmutableArray<Edge> edges = default)
    {
        Id = id;
        Edges = edges.IsDefault ? ImmutableArray<Edge>.Empty : edges;
    }

    public Relation()
    {
    }
}
