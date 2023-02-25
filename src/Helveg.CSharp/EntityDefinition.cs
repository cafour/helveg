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

    bool IsInvalid => Name == InvalidName;

    EntityToken Token { get; }

    IEntityReference GetReference();
}

public abstract record EntityDefinition : IEntityDefinition
{
    public string Name { get; init; } = IEntityDefinition.InvalidName;

    public EntityToken Token { get; init; } = EntityToken.Invalid;

    public abstract IEntityReference GetReference();
}
