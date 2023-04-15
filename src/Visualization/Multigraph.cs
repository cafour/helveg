using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record Multigraph
{
    public string Id { get; init; } = Const.Invalid;

    public string? Label { get; init; }

    public ImmutableDictionary<string, Node> Nodes { get; init; }
        = ImmutableDictionary<string, Node>.Empty;

    public ImmutableDictionary<string, Relation> Relations { get; init; }
        = ImmutableDictionary<string, Relation>.Empty;
}
