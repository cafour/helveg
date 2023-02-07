using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public abstract record HelDefinitionCS<TReference> : IHelEntityCS
    where TReference : HelReferenceCS
{
    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public bool IsDefinition => true;

    public abstract TReference Reference { get; }
}
