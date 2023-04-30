using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

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

    public NodeBuilder GetNode(string id, string? label = null, string? style = null)
    {
        var existing = Nodes.GetOrAdd(id, _ =>
        {
            var builder = new NodeBuilder { Id = id };
            if (!string.IsNullOrEmpty(label))
            {
                builder.SetProperty(Const.LabelProperty, label);
            }

            if (!string.IsNullOrEmpty(style))
            {
                builder.SetProperty(Const.StyleProperty, style);
            }

            return builder;
        });

        if (!string.IsNullOrEmpty(label)
            && existing.Properties.ContainsKey(Const.LabelProperty)
            && (existing.Properties[Const.LabelProperty] as string) != label)
        {
            existing = existing.SetProperty(Const.LabelProperty, label);
        }
        
        return existing;
    }

    public RelationBuilder GetRelation(string id)
    {
        return Relations.GetOrAdd(id, _ => new RelationBuilder { Id = id });
    }

    public MultigraphBuilder AddEdge(string relationId, Edge edge, string? style = null)
    {
        if (!string.IsNullOrEmpty(style) && !edge.Properties.ContainsKey(Const.StyleProperty))
        {
            edge = edge with
            {
                Properties = edge.Properties.SetItem(Const.StyleProperty, style)
            };
        }

        GetRelation(relationId).AddEdge(edge);

        return this;
    }

    public MultigraphBuilder AddEdges(string relationId, IEnumerable<Edge> edges, string? style = null)
    {
        foreach (var edge in edges)
        {
            AddEdge(relationId, edge, style);
        }
        return this;
    }
}
