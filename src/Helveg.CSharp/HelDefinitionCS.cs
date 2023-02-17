using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IHelDefinitionCS : IHelEntityCS
{
    IHelReferenceCS GetReference()
    {
        return new HelReferenceCS { Token = Token };
    }
}

public abstract record HelDefinitionCS : IHelDefinitionCS
{
    public string Name { get; init; } = IHelEntityCS.InvalidName;

    public HelEntityTokenCS Token { get; init; } = HelEntityTokenCS.Invalid;

    public virtual IHelReferenceCS GetReference()
    {
        return new HelReferenceCS { Token = Token };
    }
}
