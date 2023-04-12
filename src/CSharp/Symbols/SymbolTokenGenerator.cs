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

    public SymbolTokenGenerator(NumericToken containingAssembly)
    {
        ContainingAssembly = containingAssembly;
        Invalid = NumericToken.CreateInvalid(ContainingAssembly.Namespace, ContainingAssembly.Values);
        None = NumericToken.CreateNone(ContainingAssembly.Namespace, ContainingAssembly.Values);
    }

    public NumericToken ContainingAssembly { get; }

    public NumericToken Invalid { get; }

    public NumericToken None { get; }

    public NumericToken GetToken()
    {
        return ContainingAssembly.Derive(Interlocked.Increment(ref counter));
    }
}
