using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record Edge
{
    public string Src { get; init; } = Const.Invalid;

    public string Dst { get; init; } = Const.Invalid;

    public ImmutableDictionary<string, object?> Properties { get; init; }
        = ImmutableDictionary<string, object?>.Empty;

    public Edge(
        string src,
        string dst,
        ImmutableDictionary<string, object?>? properties = null)
    {
        Src = src;
        Dst = dst;

        if (properties is not null)
        {
            Properties = properties;
        }
    }

    public Edge()
    {
    }

}
