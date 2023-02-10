using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record struct HelEntityTokenCS(HelEntityKindCS Kind, int Id) : IInvalidable<HelEntityTokenCS>
{
    public static HelEntityTokenCS Invalid { get; } = new(HelEntityKindCS.Invalid, 0);

    public static HelEntityTokenCS GetUnresolved(HelEntityKindCS Kind)
    {
        return new(Kind, -1);
    }
}
