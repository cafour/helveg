using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.Visualization;

public record Node
{
    [JsonIgnore]
    public string Id { get; init; } = Const.Invalid;

    public ImmutableDictionary<string, object?> Properties { get; init; }
        = ImmutableDictionary<string, object?>.Empty;

    public Node(
        string id,
        ImmutableDictionary<string, object?>? properties = null)
    {
        Id = id;

        if (properties is not null)
        {
            Properties = properties;
        }
    }

    public Node()
    {
    }
}
