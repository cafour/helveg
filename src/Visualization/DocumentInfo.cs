using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record DocumentInfo
{
    public static DocumentInfo Invalid { get; } = new();

    [JsonIgnore]
    public bool IsValid => Name != Const.Invalid;

    public string Name { get; init; } = Const.Invalid;

    public DateTimeOffset CreatedOn { get; init; }

    public string HelvegVersion { get; init; } = GitVersionInformation.FullSemVer;

    public string? Revision { get; init; }
}
