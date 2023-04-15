using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public enum RootKind
{
    Unknown = 0,
    Solution = 1,
    Framework = 2,
    PackageRepository = 3,
    ExternalDependencySource = 4
}
