using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// A thread-safe way of generating <see cref="SymbolToken"/>s.
/// </summary>
internal class SymbolTokenGenerator
{
    private int counter = 0;

    public SymbolToken GetToken(SymbolKind kind)
    {
        return new SymbolToken(kind, Interlocked.Increment(ref counter));
    }
}
