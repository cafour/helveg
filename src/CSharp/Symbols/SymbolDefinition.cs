using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public interface ISymbolDefinition
{
    string Name { get; }

    bool IsInvalid { get; }

    SymbolToken Token { get; }

    IEntityReference GetReference();
}

/// <summary>
/// A class for sharing properties among all symbol definitions.
/// </summary>
public abstract record SymbolDefinition : ISymbolDefinition
{
    public string Name { get; init; } = CSharpConstants.InvalidName;

    public SymbolToken Token { get; init; } = SymbolToken.Invalid;

    public bool IsInvalid => Token.IsError || Name == CSharpConstants.InvalidName;

    public abstract IEntityReference GetReference();
}
