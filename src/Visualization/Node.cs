using System.Collections.Immutable;

namespace Helveg.Visualization;

public record Node
{
    public string Id { get; init; }

    public string? Label { get; init; }

    public ImmutableArray<Property> Properties { get; init; }
        = ImmutableArray<Property>.Empty;

    public Node(
        string id,
        string? label = null,
        ImmutableArray<Property> properties = default)
    {
        Id = id;
        Label = label;

        if (!properties.IsDefaultOrEmpty)
        {
            Properties = properties;
        }
    }
}
