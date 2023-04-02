using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public interface IEntityReference
{
    string? Hint { get; }
    SymbolToken Token { get; }
}

/// <summary>
/// A class for sharing properties among all entity references.
/// </summary>
public abstract record SymbolReference : IEntityReference
{
    public string? Hint { get; init; }
    public SymbolToken Token { get; init; } = SymbolToken.Invalid;
}
