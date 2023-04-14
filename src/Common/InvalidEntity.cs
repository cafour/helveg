using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg;

public sealed record InvalidEntity : EntityBase
{
    public override string Id => Const.Invalid;

    public static InvalidEntity Instance { get; } = new();
}
