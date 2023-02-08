using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public enum HelEntityKindCS
{
    Invalid = 0,
    Solution,
    Project,
    Assembly,
    Module,

    Type,
    TypeParameter,

    Field,
    Method,
    Property,
    Event,

    Parameter
}
