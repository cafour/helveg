using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record ArrayTypeReference : TypeReference
{
    public new static ArrayTypeReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Type) };

    public override TypeKind TypeKind => TypeKind.Array;

    public TypeReference ElementType { get; init; } = TypeReference.Invalid;

    public ImmutableArray<int> Sizes { get; init; } = ImmutableArray<int>.Empty;

    public int Rank { get; init; }
}
