using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Helveg;

public record Workspace
{
    private readonly ConcurrentDictionary<string, IEntity> entities = new();
    private readonly ConcurrentDictionary<string, object> scratchSpace = new();
    private readonly ConcurrentDictionary<string, IEntity> roots = new();
    private readonly ILogger<Workspace> logger;

    public IReadOnlyDictionary<string, IEntity> Roots => roots;

    public DataSource Source { get; init; } = DataSource.Invalid;

    public Workspace(ILogger<Workspace>? logger = null)
    {
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<Workspace>();
    }

    public bool TryAddRoot(IEntity root)
    {
        if (!roots.TryAdd(root.Id, root))
        {
            return false;
        }

        root.Accept(new EntityTrackingVisitor(entities));
        return true;
    }

    public void RemoveRoot(IEntity root)
    {
        if (roots.TryRemove(root.Id, out _))
        {
            root.Accept(new EntityUntrackingVisitor(entities));
        }
    }

    public void SetRoot(IEntity root)
    {
        roots.AddOrUpdate(
            key: root.Id,
            addValueFactory: _ => root,
            updateValueFactory: (_, _) => root
        );
        root.Accept(new EntityUntrackingVisitor(entities));
        root.Accept(new EntityTrackingVisitor(entities));
    }

    public IEntity? GetEntity(string id)
    {
        return entities.TryGetValue(id, out var entity) ? entity : null;
    }

    public void AddOrUpdateScratch<T>(string key, Func<string, T> addFactory, Func<string, T, T> updateFactory)
        where T : notnull
    {
        scratchSpace.AddOrUpdate(key, k => addFactory(k), (k, e) => updateFactory(k, (T)e));
    }

    public T? GetScratch<T>(string key)
    {
        return (T?)scratchSpace.GetValueOrDefault(key);
    }

    public void Accept(IEntityVisitor visitor)
    {
        foreach (var root in roots.Values)
        {
            visitor.Visit(root);
        }
    }
}
