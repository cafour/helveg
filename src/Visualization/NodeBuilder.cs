using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.Visualization;

public class NodeBuilder
{
    public string Id { get; set; } = Const.Invalid;

    public string? Label { get; set; }

    public ConcurrentDictionary<string, string?> Properties { get; }
        = new();

    public Node Build()
    {
        return new Node(
            id: Id,
            label: Label,
            properties: Properties.ToImmutableDictionary()
        );
    }

    public NodeBuilder SetProperty(string key, string? value)
    {
        Properties.AddOrUpdate(key, value, (_, _) => value);
        return this;
    }
}
