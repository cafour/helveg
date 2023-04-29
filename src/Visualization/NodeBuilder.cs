using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Helveg.Visualization;

public class NodeBuilder
{
    public string Id { get; set; } = Const.Invalid;

    public ConcurrentDictionary<string, object?> Properties { get; }
        = new();

    public Node Build()
    {
        return new Node(
            id: Id,
            properties: Properties.ToImmutableDictionary()
        );
    }

    public NodeBuilder SetProperty(string key, object? value)
    {
        Properties.AddOrUpdate(key, value, (_, _) => value);
        return this;
    }
}
