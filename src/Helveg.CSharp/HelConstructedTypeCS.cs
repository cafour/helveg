using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

// TODO: Merge into HelTypeReferenceCS to be consistent with HelMethodReferenceCS
public record HelConstructedTypeCS : HelTypeReferenceCS, IInvalidable<HelConstructedTypeCS>
{
    public new static HelConstructedTypeCS Invalid { get; } = new();

    public ImmutableArray<HelTypeReferenceCS> TypeArguments { get; init; }
        = ImmutableArray<HelTypeReferenceCS>.Empty;

    public HelTypeReferenceCS ConstructedFrom { get; init; } = HelTypeReferenceCS.Invalid;
}
