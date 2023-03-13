using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record PointerTypeReference : TypeReference
{
    public static new PointerTypeReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Type) };

    public override TypeKind TypeKind => TypeKind.Pointer;

    public TypeReference PointedAtType { get; init; } = TypeReference.Invalid;
}
