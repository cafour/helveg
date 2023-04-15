using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public record PointerTypeReference : TypeReference
{
    public static new PointerTypeReference Invalid { get; } = new();

    public override TypeKind TypeKind => TypeKind.Pointer;

    public TypeReference PointedAtType { get; init; } = TypeReference.Invalid;
}
