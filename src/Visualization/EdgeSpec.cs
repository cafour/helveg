using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record EdgeSpec
{
    public string Src { get; init; } = Const.Invalid;

    public string Dst { get; init; } = Const.Invalid;

    public EdgeSpec()
    {
    }

    public EdgeSpec(string src, string dst)
    {
        Src = src;
        Dst = dst;
    }
}
