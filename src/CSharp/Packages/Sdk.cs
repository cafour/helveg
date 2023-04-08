using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public record Sdk : EntityBase
{
    public string Name { get; init; } = Const.Invalid;
    public string Version { get; init; } = Const.Invalid;
}
