using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public class EntityTokenGenerator
{
    private int counter = 0;

    public HelEntityTokenCS GetToken(HelEntityKindCS kind)
    {
        return new HelEntityTokenCS(kind, Interlocked.Increment(ref counter));
    }
}
