using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IEntityDefinition
{
    const string InvalidName = "Invalid";

    string Name { get; }

    bool IsInvalid => Token.IsError || Name == InvalidName;

    EntityToken Token { get; }

    IEntityReference GetReference();
}

public abstract record EntityDefinition<TReference> : IEntityDefinition
    where TReference : IEntityReference
{
    public string Name { get; init; } = IEntityDefinition.InvalidName;

    public EntityToken Token { get; init; } = EntityToken.Invalid;

    IEntityReference IEntityDefinition.GetReference()
    {
        return GetReference();
    }
    public abstract TReference GetReference();
}
