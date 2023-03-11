using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// A thread-safe way of generating <see cref="EntityToken"/>s.
/// </summary>
internal class EntityTokenGenerator
{
    private int counter = 0;

    public EntityToken GetToken(EntityKind kind)
    {
        return new EntityToken(kind, Interlocked.Increment(ref counter));
    }
}
