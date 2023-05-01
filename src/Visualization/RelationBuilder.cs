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

    public Dictionary<string, Edge> Edges { get; } = new();

    public Relation Build()
    {
        return new(
            id: Id,
            edges: Edges.ToImmutableDictionary());
    }

    public bool TryAddEdge(Edge edge)
    {
        var id = $"{edge.Src};{edge.Dst}";

        if (Edges.ContainsKey(id))
        {
            return false;
        }

        Edges.Add(id, edge);
        return true;
    }

    public int TryAddEdges(IEnumerable<Edge> edges)
    {
        var count = 0;
        foreach(var edge in edges)
        {
            if (TryAddEdge(edge))
            {
                count++;
            }
        }
        return count;
    }
}
