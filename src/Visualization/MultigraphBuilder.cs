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

    public string? Label { get; set; }

    public ConcurrentDictionary<string, NodeBuilder> Nodes { get; } = new();

    public ConcurrentDictionary<string, RelationBuilder> Relations { get; } = new();

    public Multigraph Build()
    {
        return new Multigraph
        {
            Id = Id,
            Label = Label,
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

    public NodeBuilder AddNode(NodeBuilder node)
    {
        if (!Nodes.TryAdd(node.Id, node))
        {
            throw new ArgumentException($"The multigraph already contains a '{node.Id}' node.");
        }
        return node;
    }

    public NodeBuilder AddNode(string id, string? label)
    {
        return AddNode(new NodeBuilder
        {
            Id = id,
            Label = label
        });
    }

    public RelationBuilder AddRelation(string id, string? label)
    {
        var relationBuilder = new RelationBuilder
        {
            Id = id,
            Label = label
        };

        if (!Relations.TryAdd(id, relationBuilder))
        {
            throw new ArgumentException($"The multigraph already contains a '{id}' relation.");
        }

        return relationBuilder;
    }

    public RelationBuilder GetRelation(string id)
    {
        return Relations.GetOrAdd(id, _ => new RelationBuilder { Id = id });
    }

    public MultigraphBuilder AddEdge(string relationId, Edge edge)
    {
        if (!Nodes.ContainsKey(edge.Src))
        {
            throw new ArgumentException($"The multigraph doesn't contain a '{edge.Src}' node.");
        }

        if (!Nodes.ContainsKey(edge.Dst))
        {
            throw new ArgumentException($"The multigraph doesn't contain a '{edge.Dst}' node.");
        }

        GetRelation(relationId).AddEdge(edge);
        return this;
    }

    public MultigraphBuilder AddEdges(string relationId, IEnumerable<Edge> edges)
    {
        foreach(var edge in edges)
        {
            AddEdge(relationId, edge);
        }
        return this;
    }
}
