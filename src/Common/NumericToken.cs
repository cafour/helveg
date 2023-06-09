﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helveg;

/// <summary>
/// A structure that can serve as a structured identifier underlying an <see cref="IEntity.Id"/>.
/// </summary>
[JsonConverter(typeof(NumericTokenJsonConverter))]
[DebuggerDisplay("{ToString(),nq}")]
public record struct NumericToken
{
    public const char Separator = ':';
    public const int InvalidValue = -1;
    public const int NoneValue = 0;

    public string Namespace { get; init; }

    public ImmutableArray<int> Values { get; init; }
        = ImmutableArray<int>.Empty;

    public bool IsValid => !Values.IsDefaultOrEmpty && Values.Last() != InvalidValue;

    public bool IsNone => !Values.IsDefaultOrEmpty && Values.Last() == NoneValue;

    public bool HasValue => IsValid && !IsNone;

    public NumericToken Parent => Values.Length > 1
        ? Create(Namespace, Values.RemoveAt(Values.Length - 1))
        : GlobalInvalid;

    public NumericToken Derive(int index) => Create(Namespace, Values.Add(index));

    public static readonly NumericToken GlobalInvalid = CreateInvalid(Const.GlobalNamespace);

    public static readonly NumericToken GlobalNone = CreateNone(Const.GlobalNamespace);

    public NumericToken()
    {
        Namespace = Const.Invalid;
    }

    public static implicit operator string(NumericToken token)
    {
        return token.ToString();
    }

    public static NumericToken Create(string @namespace, ImmutableArray<int> values)
    {
        if (values.IsDefaultOrEmpty)
        {
            throw new ArgumentException($"A {nameof(NumericToken)} must have at least one value.", nameof(values));
        }

        if (@namespace.Contains(Separator))
        {
            throw new ArgumentException(
                $"A {nameof(NumericToken)} namespace must not contain '{Separator}'.",
                nameof(@namespace));
        }

        return new NumericToken
        {
            Namespace = @namespace,
            Values = values
        };
    }

    public static NumericToken Create(string @namespace, IEnumerable<int> values)
    {
        return Create(@namespace, values.ToImmutableArray());
    }

    public static NumericToken Create(string @namespace, params int[] values)
    {
        return Create(@namespace, values.ToImmutableArray());
    }

    public static NumericToken CreateInvalid(string @namespace, ImmutableArray<int> prefixValues)
    {
        return new NumericToken
        {
            Namespace = @namespace,
            Values = prefixValues.Add(InvalidValue)
        };
    }

    public static NumericToken CreateInvalid(string @namespace, IEnumerable<int> prefixValues)
    {
        return CreateInvalid(@namespace, prefixValues.ToImmutableArray());
    }

    public static NumericToken CreateInvalid(string @namespace, params int[] prefixValues)
    {
        return CreateInvalid(@namespace, prefixValues.ToImmutableArray());
    }

    public static NumericToken CreateNone(string @namespace, ImmutableArray<int> prefixValues)
    {
        return new NumericToken
        {
            Namespace = @namespace,
            Values = prefixValues.Add(NoneValue)
        };
    }

    public static NumericToken CreateNone(string @namespace, IEnumerable<int> prefixValues)
    {
        return CreateNone(@namespace, prefixValues.ToImmutableArray());
    }

    public static NumericToken CreateNone(string @namespace, params int[] prefixValues)
    {
        return CreateNone(@namespace, prefixValues.ToImmutableArray());
    }

    public static bool TryParse(string value, out NumericToken token)
    {
        if (string.IsNullOrEmpty(value))
        {
            token = default;
            return false;
        }

        var parts = value.Split(new[] { Separator });
        if (parts.Length < 2)
        {
            token = default;
            return false;
        }

        var @namespace = parts[0].Trim();

        var builder = ImmutableArray.CreateBuilder<int>();
        foreach(var part in parts.Skip(1))
        {
            if (!int.TryParse(part, out var number))
            {
                token = CreateInvalid(@namespace, builder);
                return false;
            }
            builder.Add(number);
        }

        token = Create(@namespace, builder.ToImmutable());
        return true;
    }

    public static bool TryParse(
        string value,
        string? @namespace,
        int length,
        ImmutableArray<int> prefixValues,
        out NumericToken token)
    {
        if (!TryParse(value, out token))
        {
            return false;
        }

        if (@namespace != null && (token.Namespace != @namespace || token.Values.Length < prefixValues.Length))
        {
            return false;
        }

        if (length > 0 && prefixValues.Length != length)
        {
            return false;
        }

        for (int i = 0; i < prefixValues.Length; ++i)
        {
            if (token.Values[i] != prefixValues[i])
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryParse(
        string value,
        string? @namespace,
        int length,
        IEnumerable<int> prefixValues,
        out NumericToken token)
    {
        return TryParse(value, @namespace, length, prefixValues.ToImmutableArray(), out token);
    }

    public static NumericToken Parse(string value)
    {
        if (!TryParse(value, out NumericToken token))
        {
            throw new ArgumentException(
                $"Value is not a valid {nameof(NumericToken)}.",
                nameof(value));
        }
        return token;
    }

    public static NumericToken Parse(
        string value,
        string? @namespace,
        int length,
        ImmutableArray<int> prefixValues)
    {
        if (!TryParse(value, @namespace, length, prefixValues, out var token))
        {
            var prefix = string.Join(":", prefixValues);
            throw new ArgumentException($"Value is not a numeric token in the '{@namespace}' namespace with " +
                $"a '{prefix}' prefix with values of length '{length}'.");

        }
        return token;
    }

    public static NumericToken Parse(
        string value,
        string? @namespace,
        int length,
        IEnumerable<int> prefixValues)
    {
        return Parse(value, @namespace, length, prefixValues.ToImmutableArray());
    }

    public static NumericToken Parse(
        string value,
        string? @namespace,
        int length,
        params int[] prefixValues)
    {
        return Parse(value, @namespace, length, prefixValues.ToImmutableArray());
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Namespace);
        foreach(var value in Values)
        {
            sb.Append(Separator);
            sb.Append(value);
        }
        return sb.ToString();
    }

    public override int GetHashCode()
    {
        var value = Namespace.GetHashCode() * 17;
        for(var i = 0; i < Values.Length; i++)
        {
            value ^= Values[i].GetHashCode();
        }
        return value;
    }

    public bool Equals(NumericToken other)
    {
        return other.Namespace.Equals(other.Namespace)
            && Values.SequenceEqual(other.Values);
    }
}

public class NumericTokenJsonConverter : JsonConverter<NumericToken>
{
    public override NumericToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new JsonException($"Could not parse an {nameof(NumericToken)}. It cannot be null.");
        
        if (!NumericToken.TryParse(value, out var token))
        {
            throw new JsonException($"Could not parse an {nameof(NumericToken)}.");
        }

        return token;
    }

    public override void Write(Utf8JsonWriter writer, NumericToken value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}