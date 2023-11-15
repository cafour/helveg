using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg;

public record DataSource(
    string Path,
    DateTimeOffset CreatedAt,
    string? CompareTo
)
{
    public static readonly DataSource Invalid = new("Invalid", default, default);
}
