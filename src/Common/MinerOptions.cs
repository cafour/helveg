using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg;

public record MinerOptions
{
    public ImmutableArray<Type> Dependencies { get; init; }
        = ImmutableArray<Type>.Empty;
}
