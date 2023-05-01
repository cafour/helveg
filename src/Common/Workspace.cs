using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg;

public record Workspace
{
    private readonly ConcurrentDictionary<string, IEntity> entities = new();
    private readonly ConcurrentDictionary<string, IEntity> roots = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> rootSemaphores = new();
    private readonly ILogger<Workspace> logger;

    public IReadOnlyDictionary<string, IEntity> Roots => roots;
    public IReadOnlyDictionary<string, IEntity> Entities => entities;

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

    public async Task<ExclusiveEntityHandle<T>> GetRootExclusively<T>(
        string id,
        CancellationToken cancellationToken = default)
        where T : IEntity
    {
        var semaphore = rootSemaphores.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new ExclusiveEntityHandle<T>(id, semaphore, CommitHandleChanges)
        {
            Entity = (T?)roots.GetValueOrDefault(id)
        };
    }

    public Task<ExclusiveEntityHandle<IEntity>> GetRootExclusively(
        string id,
        CancellationToken cancellationToken = default)
    {
        return GetRootExclusively<IEntity>(id, cancellationToken);
    }

    public void Accept(IEntityVisitor visitor)
    {
        foreach (var root in roots.Values)
        {
            visitor.Visit(root);
        }
    }

    private void CommitHandleChanges(string id, IEntity? entity)
    {
        if (entity is null)
        {
            if (roots.TryRemove(id, out entity))
            {
                entity.Accept(new EntityUntrackingVisitor(entities));
            }
        }
        else
        {
            roots.AddOrUpdate(id, entity, (_, existing) =>
            {
                entity.Accept(new EntityUntrackingVisitor(entities));
                return entity;
            });
            entity.Accept(new EntityTrackingVisitor(entities));
        }
    }
}
