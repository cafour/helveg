using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record HelPointerTypeCS : HelTypeReferenceCS, IInvalidable<HelPointerTypeCS>
{
    public static new HelPointerTypeCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.Pointer;

    public HelTypeReferenceCS PointedAtType { get; init; } = HelTypeReferenceCS.Invalid;
}
