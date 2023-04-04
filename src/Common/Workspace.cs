using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Helveg;

public record Workspace
{
    private readonly ConcurrentDictionary<string, IEntity> entities = new();
    private readonly ConcurrentDictionary<string, object> scratchSpace = new();
    private ImmutableDictionary<string, IEntity> roots = ImmutableDictionary<string, IEntity>.Empty;

    public ImmutableDictionary<string, IEntity> Roots => roots;

    public Target Target { get; init; } = Target.Invalid;

    public void AddRoot(IEntity root)
    {
        if (!ImmutableInterlocked.TryAdd(ref roots, root.Id, root))
        {
            throw new ArgumentException($"The environment already contains a root with the '{root.Id}' id.");
        }

        root.Accept(new EntityTrackingVisitor(entities));
    }

    public void RemoveRoot(IEntity root)
    {
        if (!ImmutableInterlocked.TryRemove(ref roots, root.Id, out _))
        {
            throw new ArgumentException($"A root with the '{root.Id}' id could not be removed.");
        }

        root.Accept(new EntityUntrackingVisitor(entities));
    }

    public void SetRoot(IEntity root)
    {
        ImmutableInterlocked.AddOrUpdate(
            location: ref roots,
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
}
