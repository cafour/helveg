using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Helveg.Serialization
{
    public static class Serialize
    {
        public static readonly JsonSerializerOptions JsonOptions;

        static Serialize()
        {
            JsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            JsonOptions.Converters.Add(new Vector3JsonConverter());
            JsonOptions.Converters.Add(new Vector2JsonConverter());
        }

        public static async Task<T?> GetCache<T>(string path, ILogger? logger)
            where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using var stream = new FileStream(path, FileMode.Open);
                return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
            }
            catch(JsonException e)
            {
                logger?.LogDebug(e, $"Failed to read the '{path}' cache.");
            }
            return null;
        }

        public static async Task SetCache<T>(string path, T value, ILogger? logger)
        {
            using var stream = new FileStream(path, FileMode.Create);
            logger?.LogDebug($"Caching to '{path}'.");
            await JsonSerializer.SerializeAsync(stream, value, JsonOptions);
        }
    }
}
