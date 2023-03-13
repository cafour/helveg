using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IEntityDefinition
{
    string Name { get; }

    bool IsInvalid { get; }

    EntityToken Token { get; }

    IEntityReference GetReference();
}

/// <summary>
/// A class for sharing properties among all entity definitions.
/// </summary>
public abstract record EntityDefinition : IEntityDefinition
{
    public string Name { get; init; } = CSharpConstants.InvalidName;

    public EntityToken Token { get; init; } = EntityToken.Invalid;

    public bool IsInvalid => Token.IsError || Name == CSharpConstants.InvalidName;

    public abstract IEntityReference GetReference();
}
