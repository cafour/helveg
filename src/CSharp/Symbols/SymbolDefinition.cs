using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public interface ISymbolDefinition : IEntity
{
    string Name { get; }

    [JsonIgnore]
    bool IsValid { get; }

    [JsonIgnore]
    NumericToken Token { get; }

    ISymbolReference GetReference();
}

/// <summary>
/// A base class for all symbol definitions.
/// </summary>
public abstract record SymbolDefinition : EntityBase, ISymbolDefinition
{
    [JsonIgnore]
    public bool IsValid => Token.IsValid || Name != Const.Invalid;

    public string Name { get; init; } = Const.Invalid;

    [JsonIgnore]
    public NumericToken Token { get; init; } = CSConst.InvalidToken;

    public override string Id
    {
        get => Token;
        init
        {
            if (!NumericToken.TryParse(value, out NumericToken token))
            {
                throw new ArgumentException("Value is not a valid numeric token.");
            }
            Token = token;
        }
    }

    public abstract ISymbolReference GetReference();
}
