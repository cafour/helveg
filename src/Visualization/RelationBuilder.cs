using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public class RelationBuilder
{
    public string Id { get; set; } = Const.Invalid;

    public string? Label { get; set; }

    public ConcurrentDictionary<EdgeSpec, Edge> Edges { get; } = new();

    public Relation Build()
    {
        return new(
            id: Id,
            label: Label,
            edges: Edges.ToImmutableDictionary());
    }

    public RelationBuilder AddEdge(Edge edge)
    {
        var spec = new EdgeSpec(edge.Src, edge.Dst);
        if (!Edges.TryAdd(spec, edge))
        {
            throw new ArgumentException($"The relation already contains an '{edge.Src}' -> '{edge.Dst}'.");
        }

        return this;
    }

    public RelationBuilder AddEdges(IEnumerable<Edge> edges)
    {
        foreach(var edge in edges)
        {
            AddEdge(edge);
        }
        return this;
    }
}
