using System;
using System.Collections;
using System.Collections.Generic;

namespace Helveg.Visualization;

public static class MultigraphExtensions
{
    public static TNode GetNode<TNode>(this Multigraph graph, string id, string? name = null)
        where TNode : MultigraphNode, new()
    {
        if (graph.Nodes.TryGetValue(id, out var node))
        {
            node.Name = name ?? node.Name;
            return (TNode)node;
        }

        node = new TNode
        {
            Name = name!
        };
        graph.Nodes.Add(id, node);
        return (TNode)node;
    }

    public static void AddEdges(
        this Multigraph graph,
        string relationName,
        IEnumerable<MultigraphEdge> edges,
        bool isTransitive = false)
    {
        if (!graph.Relations.TryGetValue(relationName, out var relation))
        {
            relation = new MultigraphRelation
            {
                Name = relationName,
                IsTransitive = isTransitive,
                Edges = new()
            };
            graph.Relations.Add(relationName, relation);
        }

        foreach (var edge in edges)
        {
            relation.Edges.Add($"{edge.Src};{edge.Dst}", edge);
        }
    }

    public static void AddEdge(
        this Multigraph graph,
        string relationName,
        MultigraphEdge edge,
        bool isTransitive = false)
    {
        graph.AddEdges(relationName, new[] { edge }, isTransitive);
    }

    public static MultigraphDiagnostic ToMultigraphDiagnostic(this Diagnostic diagnostic)
    {
        return new MultigraphDiagnostic
        {
            Id = diagnostic.Id,
            Message = diagnostic.Message,
            Severity = diagnostic.Severity.ToMultigraphDiagnosticSeverity()
        };
    }

    public static MultigraphDiagnosticSeverity ToMultigraphDiagnosticSeverity(this DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Hidden => MultigraphDiagnosticSeverity.Hidden,
            DiagnosticSeverity.Info => MultigraphDiagnosticSeverity.Info,
            DiagnosticSeverity.Warning => MultigraphDiagnosticSeverity.Warning,
            DiagnosticSeverity.Error => MultigraphDiagnosticSeverity.Error,
            _ => throw new ArgumentException($"Severity '{severity}' is not supported.")
        };
    }

    public static MultigraphComment ToMultigraphComment(this Comment comment)
    {
        return comment.Format switch
        {
            CommentFormat.Plain => new MultigraphComment
            {
                Content = comment.Content,
                Format = MultigraphCommentFormat.Plain
            },
            CommentFormat.Markdown => new MultigraphComment
            {
                Content = comment.Content,
                Format = MultigraphCommentFormat.Markdown,
            },
            _ => throw new NotSupportedException($"The '{comment.Format}' comment format is unsupported.")
        };
    }
    
    public static MultigraphNodeDiffStatus? ToMultigraphDiffStatus(this DiffStatus diff)
    {
        return diff switch
        {
            DiffStatus.Unmodified => null,
            DiffStatus.Modified => MultigraphNodeDiffStatus.Modified,
            DiffStatus.Added => MultigraphNodeDiffStatus.Added,
            DiffStatus.Deleted => MultigraphNodeDiffStatus.Deleted,
            _ => throw new NotSupportedException()
        };
    }
}
