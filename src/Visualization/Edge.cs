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

    public string? Label { get; init; }

    public ImmutableDictionary<string, string> Properties { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public Edge(
        string src,
        string dst,
        string? label = null,
        ImmutableDictionary<string, string>? properties = null)
    {
        Src = src;
        Dst = dst;
        Label = label;

        if (properties is not null)
        {
            Properties = properties;
        }
    }

    public Edge()
    {
    }

}
