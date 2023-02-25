using Helveg.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// A structure that uniquely identifies an <see cref="IEntityDefinition"/> in a <see cref="EntityWorkspace"/>.
/// </summary>
[JsonConverter(typeof(EntityTokenJsonConverter))]
public record struct EntityToken(EntityKind Kind, int Id) : IInvalidable<EntityToken>
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    public const int ErrorId = -1;

    public static EntityToken Invalid { get; } = new(EntityKind.Unknown, ErrorId);

    public static EntityToken CreateError(EntityKind kind)
    {
        return new EntityToken(kind, ErrorId);
    }

    [JsonIgnore]
    public bool IsError => Id == ErrorId;

    public override string ToString()
    {
        uint id = (uint)Id;
        var sb = new StringBuilder(Kind.ToString());
        sb.Append('-');
        var index = sb.Length;
        do
        {
            sb.Insert(index, Alphabet[(int)(id % 64)]);
            id /= 64;
        }
        while (id != 0);
        return sb.ToString();
    }

    public static bool TryParse(string value, out EntityToken token)
    {
        var parts = value.Split('-', 2);
        if (parts.Length != 2 || !Enum.TryParse<EntityKind>(parts[0], out var kind))
        {
            token = Invalid;
            return false;
        }

        uint id = 0;
        for (int i = 0; i < parts[1].Length; ++i)
        {
            id *= 64;
            var index = Alphabet.IndexOf(parts[1][i]);
            if (index == -1)
            {
                token = Invalid;
                return false;
            }

            id += (uint)index;
        }

        token = new(kind, (int)id);
        return true;
    }
}

public class EntityTokenJsonConverter : JsonConverter<EntityToken>
{
    public override EntityToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
        {
            return EntityToken.Invalid;
        }

        if (EntityToken.TryParse(value, out var token))
        {
            return token;
        }

        throw new JsonException($"Could not parse an {nameof(EntityToken)}.");
    }

    public override void Write(Utf8JsonWriter writer, EntityToken value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
