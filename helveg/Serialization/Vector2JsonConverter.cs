using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helveg.Serialization
{
    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("A Vector3 must start as an array.");
            }

            reader.Read();
            var x = reader.GetSingle();
            reader.Read();
            var y = reader.GetSingle();
            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException("A Vector3 must end as an array.");
            }
            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
