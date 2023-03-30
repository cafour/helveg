using System.Collections.Immutable;

namespace Helveg.Visualization;

public record Node
{
    public string Id { get; init; }

    public string? Label { get; init; }

    public ImmutableDictionary<string, string> Properties { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public Node(
        string id,
        string? label = null,
        ImmutableDictionary<string, string>? properties = null)
    {
        Id = id;
        Label = label;

        if (properties is not null)
        {
            Properties = properties;
        }
    }
}
