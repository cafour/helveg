using System;
using System.Collections.Immutable;

namespace Helveg.Landscape
{
    public delegate ImmutableArray<LSymbol<TKind>> LRule<TKind>(ImmutableArray<float> parameters)
        where TKind : Enum;
}
