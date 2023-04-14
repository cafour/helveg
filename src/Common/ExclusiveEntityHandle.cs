using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg;

/// <summary>
/// Represents an operation on an entity that is guaranteed to be atomic.
/// Must be disposed to release the lock on the root and to commit the changes.
/// </summary>
public sealed class ExclusiveEntityHandle<T> : IDisposable
    where T : IEntity
{
    private readonly SemaphoreSlim? semaphore;
    private readonly Action<string, IEntity?> commitCallback;

    public ExclusiveEntityHandle(string id, SemaphoreSlim semaphore, Action<string, IEntity?> commitCallback)
    {
        Id = id;
        this.semaphore = semaphore;
        this.commitCallback = commitCallback;
    }

    public static readonly ExclusiveEntityHandle<T> Invalid = new(Const.Invalid, null!, (_, _) => { });

    public bool IsInvalid => Id == Const.Invalid;

    public string Id { get; }

    public T? Entity { get; set; }

    public void Dispose()
    {
        commitCallback(Id, Entity);
        semaphore?.Release();
    }
}
