using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IEntityReference
{
    string? Hint { get; }
    EntityToken Token { get; }
}

public abstract record EntityReference : IEntityReference
{
    public string? Hint { get; init; }
    public EntityToken Token { get; init; } = EntityToken.Invalid;
}
