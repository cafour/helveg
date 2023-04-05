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

    public ImmutableDictionary<EdgeSpec, Edge> Edges { get; init; }
        = ImmutableDictionary<EdgeSpec, Edge>.Empty;

    public Relation(string id, string? label = null, ImmutableDictionary<EdgeSpec, Edge>? edges = null)
    {
        Id = id;
        Label = label;
        Edges = edges ?? ImmutableDictionary<EdgeSpec, Edge>.Empty;
    }

    public Relation()
    {
    }
}
