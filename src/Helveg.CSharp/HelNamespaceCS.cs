using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Helveg.CSharp;

public record HelNamespaceCS : HelSymbolBaseCS
{
    public static readonly HelNamespaceCS Invalid = new();

    public override HelSymbolKindCS Kind => HelSymbolKindCS.Namespace;

    public ImmutableArray<HelTypeCS> TypeMembers { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public ImmutableArray<HelNamespaceCS> NamespaceMembers { get; init; } = ImmutableArray<HelNamespaceCS>.Empty;

    public bool IsGlobalNamespace => ContainingNamespace is null;

    public HelNamespaceKindCS NamespaceKind { get; init; }

    public IEnumerable<IHelSymbolCS> GetAllSymbols()
    {
        return Types.SelectMany(t => t.Members);
    }
}
