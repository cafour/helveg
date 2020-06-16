using System.Text.Json;

namespace Helveg.Serialization
{
    public static class Serialize
    {
        public static readonly JsonSerializerOptions JsonOptions;

        static Serialize()
        {
            JsonOptions = new JsonSerializerOptions();
            JsonOptions.Converters.Add(new Vector3JsonConverter());
            JsonOptions.Converters.Add(new Vector2JsonConverter());
        }
    }
}
