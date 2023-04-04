using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg;

public record Target(
    string Path,
    DateTimeOffset CreatedAt
)
{
    public static readonly Target Invalid = new("Invalid", default);
}
