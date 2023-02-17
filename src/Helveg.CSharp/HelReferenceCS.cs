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
}

public record HelReferenceCS: IHelReferenceCS, IInvalidable<HelReferenceCS>
{
    public static HelReferenceCS Invalid { get; } = new();

    public HelEntityTokenCS Token { get; set; } = HelEntityTokenCS.Invalid;
}
