using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record FunctionPointerTypeReference : TypeReference
{
    public new static FunctionPointerTypeReference Invalid { get; }
        = new() { Token = EntityToken.CreateError(EntityKind.Type) };

    public override TypeKind TypeKind => TypeKind.FunctionPointer;

    // TODO: Test what Roslyn actually puts in here
    public MethodReference Signature { get; init; } = MethodReference.Invalid;
}
