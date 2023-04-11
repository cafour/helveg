using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public record Dependency
{
    public string Name { get; init; } = Const.Invalid;
}
