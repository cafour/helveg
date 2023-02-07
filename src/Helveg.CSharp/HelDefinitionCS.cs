using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public abstract record HelDefinitionCS : IHelEntityCS
{
    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public bool IsDefinition => true;
}

public abstract record HelDefinitionCS<TReference> : HelDefinitionCS
    where TReference : HelReferenceCS
{
    public abstract TReference Reference { get; }
}
