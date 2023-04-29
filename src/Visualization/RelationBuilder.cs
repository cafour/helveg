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

    public List<Edge> Edges { get; } = new();

    public Relation Build()
    {
        return new(
            id: Id,
            edges: Edges.ToImmutableArray());
    }

    public RelationBuilder AddEdge(Edge edge)
    {
        Edges.Add(edge);
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
