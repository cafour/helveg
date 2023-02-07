using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record HelConstructedTypeCS : HelTypeReferenceCS, IInvalidable<HelConstructedTypeCS>
{
    public new static HelConstructedTypeCS Invalid { get; } = new();

    public ImmutableArray<HelTypeCS> TypeArguments { get; init; } = ImmutableArray<HelTypeCS>.Empty;

    public HelTypeReferenceCS ConstructedFrom { get; init; } = HelTypeReferenceCS.Invalid;
}
