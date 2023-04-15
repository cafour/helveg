using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.UI;

public record Icon(string Name, IconFormat Format, string Data)
{
    public static async Task<Icon> LoadEmbeddedSvg(string name, Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new ArgumentException($"Embedded resource '{resourceName}' could not be found.");
        }

        return await LoadSvg(name, stream);
    }

    public static async Task<Icon> LoadSvg(string name, Stream stream)
    {
        using var reader = new StreamReader(stream);
        return new Icon(name, IconFormat.Svg, await reader.ReadToEndAsync());
    }

    public static async Task<Icon> LoadEmbeddedPng(string name, Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new ArgumentException($"Embedded resource '{resourceName}' could not be found.");
        }
        return await LoadPng(name, stream);
    }

    public static async Task<Icon> LoadPng(string name, Stream stream)
    {
        using var memoryStream = new MemoryStream((int)stream.Length);
        await stream.CopyToAsync(memoryStream);
        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        return new Icon(name, IconFormat.Png, base64);
    }
}
