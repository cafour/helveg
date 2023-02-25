using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// A structure that uniquely identifies an <see cref="IEntityDefinition"/> in a <see cref="EntityWorkspace"/>.
/// </summary>
public record struct EntityToken(EntityKind Kind, int Id) : IInvalidable<EntityToken>
{
    public const int ErrorId = -1;

    public static EntityToken Invalid { get; } = new(EntityKind.Unknown, ErrorId);

    public static EntityToken CreateError(EntityKind kind)
    {
        return new EntityToken(kind, ErrorId);
    }

    [JsonIgnore]
    public bool IsError => Id == ErrorId;
}
