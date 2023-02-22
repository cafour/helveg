using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IHelReferenceCS
{
    HelEntityTokenCS Token { get; }
    string? Hint { get; }
}

public abstract record HelReferenceCS : IHelReferenceCS
{
    public string? Hint { get; init; }
    public HelEntityTokenCS Token { get; init; } = HelEntityTokenCS.Invalid;
}
