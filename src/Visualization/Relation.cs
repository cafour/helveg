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

    public string? Label { get; init; }

    public ImmutableArray<Edge> Edges { get; init; }
        = ImmutableArray<Edge>.Empty;

    public Relation(string id, string? label = null, ImmutableArray<Edge> edges = default)
    {
        Id = id;
        Label = label;
        Edges = edges.IsDefault ? ImmutableArray<Edge>.Empty : edges;
    }

    public Relation()
    {
    }
}
