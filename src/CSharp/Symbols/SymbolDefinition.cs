using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public interface ISymbolDefinition : IEntity
{
    string Name { get; }

    bool IsInvalid { get; }

    SymbolToken Token { get; }

    ISymbolReference GetReference();
}

/// <summary>
/// A base class for all symbol definitions.
/// </summary>
public abstract record SymbolDefinition : EntityBase, ISymbolDefinition
{
    public string Name { get; init; } = Const.Invalid;

    public SymbolToken Token { get; init; } = SymbolToken.Invalid;

    public bool IsInvalid => Token.IsError || Name == Const.Invalid;

    string IEntity.Id => Token.ToString();

    public abstract ISymbolReference GetReference();
}
