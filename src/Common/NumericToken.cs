using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg;

/// <summary>
/// A structure that uniquely identifies an <see cref="ISymbolDefinition"/> in a <see cref="EntityWorkspace"/>.
/// </summary>
[JsonConverter(typeof(NumericTokenJsonConverter))]
[DebuggerDisplay("{ToString(),nq}")]
public record struct NumericToken
{
    public const char NamespaceSeparator = ':';
    public const char ValueSeparator = '-';

    public const int InvalidValue = -1;

    public string Namespace { get; init; }

    public string? Kind { get; init; }

    public int Value { get; init; }

    public static NumericToken Create(string @namespace, string kind, int value)
    {
        return new NumericToken
        {
            Namespace = @namespace,
            Kind = kind,
            Value = value
        };
    }

    public static NumericToken Create(string @namespace, int value)
    {
        return new NumericToken
        {
            Namespace = @namespace,
            Value = value
        };
    }

    public static NumericToken Create<T>(string @namespace, int value)
    {
        return new NumericToken
        {
            Namespace = @namespace,
            Kind = typeof(T).Name,
            Value = value
        };
    }

    public static NumericToken CreateInvalid(string @namspace, string kind)
    {
        return new NumericToken
        {
            Namespace = @namspace,
            Kind = kind,

        };
    }

    public static NumericToken CreateInvalid(string @namespace)
    {
        return new NumericToken
        {
            Namespace = @namespace,
            Value = -1
        };
    }
}

public class NumericTokenJsonConverter : JsonConverter<NumericToken>
{
    public override NumericToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, NumericToken value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}