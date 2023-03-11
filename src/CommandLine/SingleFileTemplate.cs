using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Helveg.Visualization;

namespace Helveg;

public static class SingleFileTemplate
{
    private const string CssResource = "helveg.css";
    private const string JsResource = "helveg.js";
    
    public static string Create(Multigraph multigraph)
    {
        return
@$"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <title>{multigraph.Label ?? multigraph.Id} | Helveg</title>
        <meta charset=""utf-8"" />
        <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
        <style>
            {GetResource(CssResource)}
        </style>
    </head>

    <body>
        <div id=""sigma-container"">
        </div>
        <script type=""application/json"" id=""helveg-data"">
            {JsonSerializer.Serialize(multigraph, HelvegDefaults.JsonOptions)}
        </script>
        <script>
            {GetResource(JsResource)}
        </script>
    </body>
</html>
";
    }

    private static string GetResource(string name)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (dir is not null)
        {
            var namePath = Path.Combine(dir, name);
            if (File.Exists(namePath))
            {
                return File.ReadAllText(namePath);
            }
        }
        
        using var nameStream = typeof(SingleFileTemplate).Assembly.GetManifestResourceStream(name);
        if (nameStream is null)
        {
            throw new ArgumentException($"Could not find resource '{name}'.");
        }

        using var reader = new StreamReader(nameStream);
        return reader.ReadToEnd();
    }
}
