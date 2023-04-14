using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg;

/// <summary>
/// Represents an update operation to a <see cref="Workspace"/> root that is guaranteed to be atomic.
/// Must be disposed to release the lock on the root and to commit the changes.
/// </summary>
public sealed class ExclusiveRootHandle : IDisposable
{
    private readonly SemaphoreSlim semaphore;
    private readonly Action<string, IEntity?> commitCallback;

    public ExclusiveRootHandle(string id, Action<string, IEntity?> commitCallback)
    {
        semaphore = new(1);
        Id = id;
        this.commitCallback = commitCallback;
    }

    public string Id { get; }

    public IEntity? Root { get; set; } = InvalidEntity.Instance;

    public Task Wait(CancellationToken cancellationToken)
    {
        return semaphore.WaitAsync(cancellationToken);
    }

    public void Dispose()
    {
        commitCallback(Id, Root);
        semaphore.Release();
        semaphore.Dispose();
    }
}
