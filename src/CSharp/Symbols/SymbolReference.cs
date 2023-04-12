using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public interface ISymbolReference
{
    string? Hint { get; }
    NumericToken Token { get; }
    ImmutableArray<Diagnostic> Diagnostics { get; }
}

/// <summary>
/// A class for sharing properties among all entity references.
/// </summary>
public abstract record SymbolReference : ISymbolReference
{
    public string? Hint { get; init; }
    public NumericToken Token { get; init; } = CSConst.InvalidToken;
    public ImmutableArray<Diagnostic> Diagnostics { get; init; }
        = ImmutableArray<Diagnostic>.Empty;
}
