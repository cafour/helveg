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
    private readonly ConcurrentDictionary<string, ExclusiveRootHandle> rootHandles = new();
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

    public async Task<ExclusiveRootHandle?> GetRootExclusively(string id, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (rootHandles.TryGetValue(id, out var existingHandle))
            {
                await existingHandle.Wait(cancellationToken);
            }

            var handle = new ExclusiveRootHandle(id, CommitHandleChanges);
            if (rootHandles.TryAdd(id, handle))
            {
                handle.Root = Roots.GetValueOrDefault(id);
                return handle;
            }
        }

        return null;
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
            roots.TryRemove(id, out _);
        }
        else
        {
            roots.AddOrUpdate(id, entity, (_, _) => entity);
        }

        rootHandles.TryRemove(id, out _);
    }
}
