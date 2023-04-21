using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.Visualization;

public record Node
{
    [JsonIgnore]
    public string Id { get; init; } = Const.Invalid;

    public string? Label { get; init; }

    public ImmutableDictionary<string, object?> Properties { get; init; }
        = ImmutableDictionary<string, object?>.Empty;

    public Node(
        string id,
        string? label = null,
        ImmutableDictionary<string, object?>? properties = null)
    {
        Id = id;
        Label = label;

        if (properties is not null)
        {
            Properties = properties;
        }
    }

    public Node()
    {
    }
}
