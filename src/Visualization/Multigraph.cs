using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record Multigraph
{
    public static Multigraph Invalid { get; } = new();

    [JsonIgnore]
    public bool IsValid => Id != Const.Invalid;

    public string Id { get; init; } = Const.Invalid;

    public ImmutableDictionary<string, Node> Nodes { get; init; }
        = ImmutableDictionary<string, Node>.Empty;

    public ImmutableDictionary<string, Relation> Relations { get; init; }
        = ImmutableDictionary<string, Relation>.Empty;
}
