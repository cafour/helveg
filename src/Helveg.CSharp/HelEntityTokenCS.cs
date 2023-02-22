using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public record struct HelEntityTokenCS(HelEntityKindCS Kind, int Id) : IInvalidable<HelEntityTokenCS>
{
    public const int ErrorId = -1;

    public static HelEntityTokenCS Invalid { get; } = new(HelEntityKindCS.Unknown, ErrorId);

    public static HelEntityTokenCS CreateError(HelEntityKindCS kind)
    {
        return new HelEntityTokenCS(kind, ErrorId);
    }

    [JsonIgnore]
    public bool IsError => Id == ErrorId;
}
