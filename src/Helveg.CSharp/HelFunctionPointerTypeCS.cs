using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record HelFunctionPointerTypeCS : HelTypeReferenceCS, IInvalidable<HelFunctionPointerTypeCS>
{
    public new static HelFunctionPointerTypeCS Invalid { get; } = new();

    public override HelTypeKindCS TypeKind => HelTypeKindCS.FunctionPointer;

    public HelMethodReferenceCS Signature { get; init; } = HelMethodReferenceCS.Invalid;
}
