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
    Namespace,

    Type,
    TypeParameter,

    Field,
    Method,
    Property,
    Event,

    Parameter
}
