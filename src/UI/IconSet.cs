using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.UI;

public record IconSet(string Namespace, ImmutableDictionary<string, Icon> Icons)
{
    public static async Task<IconSet> LoadFromAssembly(string @namespace, Assembly assembly)
    {
        var iconTasks = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith("Icons"))
            .Select(n =>
            {
                var segments = n.Split('.');
                if (segments.Length < 3)
                {
                    throw new ArgumentException($"Embedded icon '{n}' does not follow the " +
                        "`Icons.*.{svg,png}` pattern.");
                }
                var extension = segments.Last();
                var fileName = segments[^2];
                return extension switch
                {
                    "svg" => Icon.LoadEmbeddedSvg(fileName, assembly, n),
                    "png" => Icon.LoadEmbeddedPng(fileName, assembly, n),
                    _ => throw new NotSupportedException($"Icons with the '.{extension}' extension are not supported.")
                };
            });
        var icons = await Task.WhenAll(iconTasks);
        return new IconSet(@namespace, icons.ToImmutableDictionary(i => i.Name));
    }
}
