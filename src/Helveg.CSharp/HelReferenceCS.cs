using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public abstract record HelReferenceCS : IHelEntityCS
{
    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public bool IsDefinition => false;

    public HelEntityTokenCS DefinitionToken { get; set; } = HelEntityTokenCS.Invalid;
}
