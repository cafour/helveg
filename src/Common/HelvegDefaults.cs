using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Helveg;

public static class HelvegDefaults
{
    public static readonly JsonSerializerOptions JsonOptions;

    static HelvegDefaults()
    {
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        ApplyJsonDefaults(JsonOptions);
    }
    
    public static JsonSerializerOptions ApplyJsonDefaults(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new JsonStringEnumConverter());
        options.TypeInfoResolver = new HelvegAssemblyTypeResolver();
        return options;
    }

    private class HelvegAssemblyTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = base.GetTypeInfo(type, options);
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return typeInfo;
            }

            var helvegAssemblies = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                .GetReferencedAssemblies()
                .Where(a => a.Name is not null && a.Name.StartsWith(nameof(Helveg)))
                .Select(Assembly.Load);
            var derivedTypes = helvegAssemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.BaseType == type)
                .Select(t => new JsonDerivedType(t, t.Name))
                .ToList();
            if (derivedTypes.Count == 0)
            {
                return typeInfo;
            }

            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };
            foreach (var derivedType in derivedTypes)
            {
                typeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
            }

            return typeInfo;
        }

    }
}
