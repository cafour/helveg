using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record HelArrayTypeCS : HelTypeReferenceCS, IInvalidable<HelArrayTypeCS>
{
    public new static HelArrayTypeCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.Array;

    public HelTypeReferenceCS ElementType { get; init; } = HelTypeReferenceCS.Invalid;

    public ImmutableArray<int> Sizes { get; init; } = ImmutableArray<int>.Empty;

    public int Rank { get; init; }
}
