using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IHelReferenceCS
{
    bool IsError { get; }
    HelEntityTokenCS Token { get; }
}

public abstract record HelReferenceCS : IHelReferenceCS
{
    public bool IsError { get; init; }
    public HelEntityTokenCS Token { get; init; } = HelEntityTokenCS.Invalid;
}
