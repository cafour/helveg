using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public class MultigraphBuilder
{
    public string Id { get; set; } = Const.Unknown;

    public ConcurrentDictionary<string, NodeBuilder> Nodes { get; } = new();

    public ConcurrentDictionary<string, RelationBuilder> Relations { get; } = new();

    public Multigraph Build()
    {
        return new Multigraph
        {
            Id = Id,
            Nodes = Nodes.ToImmutableDictionary(
                p => p.Key,
                p => p.Value.Build()
            ),
            Relations = Relations.ToImmutableDictionary(
                p => p.Key,
                p => p.Value.Build()
            )
        };
    }

    public NodeBuilder GetNode(string id, string? label = null)
    {
        return Nodes.GetOrAdd(id, _ =>
        {
            var builder = new NodeBuilder { Id = id };
            if (!string.IsNullOrEmpty(label))
            {
                builder.SetProperty(Const.LabelProperty, label);
            }

            return builder;
        });
    }

    public RelationBuilder GetRelation(string id)
    {
        return Relations.GetOrAdd(id, _ => new RelationBuilder { Id = id });
    }

    public MultigraphBuilder AddEdge(string relationId, Edge edge)
    {
        GetRelation(relationId).AddEdge(edge);
        return this;
    }

    public MultigraphBuilder AddEdges(string relationId, IEnumerable<Edge> edges)
    {
        foreach (var edge in edges)
        {
            AddEdge(relationId, edge);
        }
        return this;
    }
}
