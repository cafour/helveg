using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record Edge
{
    public string Src { get; init; }

    public string Dst { get; init; }

    public string? Label { get; init; }

    public ImmutableArray<Property> Properties { get; init; }
        = ImmutableArray<Property>.Empty;

    public Edge(
        string src,
        string dst,
        string? label = null,
        ImmutableArray<Property> properties = default)
    {
        Src = src;
        Dst = dst;
        Label = label;

        if (!properties.IsDefaultOrEmpty)
        {
            Properties = properties;
        }
    }
}
