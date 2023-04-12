using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// A structure that uniquely identifies an <see cref="ISymbolDefinition"/> in a <see cref="EntityWorkspace"/>.
/// </summary>
[JsonConverter(typeof(EntityTokenJsonConverter))]
[DebuggerDisplay("{ToString(),nq}")]
public record struct SymbolToken
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    public const int ErrorId = -1;

    public static SymbolToken Invalid { get; } = new(Const.Invalid, SymbolKind.Unknown, ErrorId);

    public SymbolKind Kind { get; init; }

    public int Id { get; init; }

    public string Prefix { get; set; }

    [JsonIgnore]
    public bool IsError => Id == ErrorId;

    private readonly Lazy<string> encodedValue;

    public SymbolToken(string prefix, SymbolKind kind, int id)
    {
        Prefix = prefix;
        Kind = kind;
        Id = id;
        encodedValue = new(Encode);
    }

    public static SymbolToken CreateError(SymbolKind kind)
    {
        return new SymbolToken(Const.Invalid, kind, ErrorId);
    }

    public static implicit operator string(SymbolToken token)
    {
        return token.encodedValue.Value;
    }

    public override string ToString()
    {
        return encodedValue.Value;
    }

    public static bool TryParse(string value, out SymbolToken token)
    {
        if (!value.StartsWith($"{CSConst.CSharpPrefix}:"))
        {
            token = Invalid;
            return false;
        }

        var parts = value[CSConst.CSharpPrefix.Length..].Split(new[] { '-' }, 3);
        if (parts.Length != 3 || !Enum.TryParse<SymbolKind>(parts[0], out var kind))
        {
            token = Invalid;
            return false;
        }

        var prefix = parts[1];

        uint id = 0;
        for (int i = 0; i < parts[2].Length; ++i)
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

        token = new(prefix, kind, (int)id);
        return true;
    }

    private string Encode()
    {
        uint id = (uint)Id;
        var sb = new StringBuilder(CSConst.CSharpPrefix);
        sb.Append(':');
        sb.Append(Kind.ToString());
        sb.Append('-');
        sb.Append(Prefix);
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
}

public class EntityTokenJsonConverter : JsonConverter<SymbolToken>
{
    public override SymbolToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
        {
            return SymbolToken.Invalid;
        }

        if (SymbolToken.TryParse(value, out var token))
        {
            return token;
        }

        throw new JsonException($"Could not parse an {nameof(SymbolToken)}.");
    }

    public override void Write(Utf8JsonWriter writer, SymbolToken value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
