using System.Collections.Immutable;

namespace Helveg.Visualization;

public record Node
{
    public string Id { get; init; }

    public string? Label { get; init; }

    public ImmutableArray<MetadataProperty> Properties { get; init; }
        = ImmutableArray<MetadataProperty>.Empty;

    public Node(
        string id,
        string? label = null,
        ImmutableArray<MetadataProperty> properties = default)
    {
        Id = id;
        Label = label;

        if (!properties.IsDefaultOrEmpty)
        {
            Properties = properties;
        }
    }
}
