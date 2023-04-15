using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Helveg.Visualization;

namespace Helveg.UI;

public class UIBuilder
{
    private const string CssResource = "helveg.css";
    private const string JsResource = "helveg.js";
    private const string IconBaseNamespace = "base";

    public List<string> Styles {get; } = new();
    public List<string> Scripts {get;}= new();
    private readonly List<IconSet> iconSets = new();
    private VisualizationModel? visualizationModel;

    public static async Task<UIBuilder> CreateDefault()
    {
        return new UIBuilder()
            .AddStyle(await GetBaseResource(CssResource))
            .AddScript(await GetBaseResource(JsResource))
            .AddIconSet(await IconSet.LoadFromAssembly(IconBaseNamespace, typeof(Icon).Assembly));
    }

    public UIBuilder AddStyle(string style)
    {
        styles.Add(style);
        return this;
    }

    public UIBuilder AddScript(string script)
    {
        Scripts.Add(script);
        return this;
    }

    public UIBuilder AddIconSet(IconSet iconSet)
    {
        iconSets.Add(iconSet);
        return this;
    }

    public UIBuilder SetVisualizationModel(VisualizationModel visualizationModel)
    {
        this.visualizationModel = visualizationModel;
        return this;
    }

    public async Task Build()
    {
        using var writer = new StringWriter();
        await Build(writer);
    }

    public async Task Build(Func<string, TextWriter> writerFactory)
    {
        if(visualizationModel is null)
        {
            throw new InvalidOperationException("A visualization model must be set first.");
        }

        writer.Write(
@$"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <title>{visualizationModel.DocumentInfo.Name ?? "Unknown"} | Helveg</title>
        <meta charset=""utf-8"" />
        <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
        ");
        foreach (var style in styles)
        {
            await writer.WriteAsync(
@$"
        <style>
            {style}
        </style>
");
        }

        await writer.WriteAsync(
@$"
    </head>

    <body>
        <div id=""app""></div>
        <script type=""application/json"" id=""helveg-data"">
            {JsonSerializer.Serialize(visualizationModel, HelvegDefaults.JsonOptions)}
        </script>
");

        foreach (var iconSet in iconSets)
        {
            await writer.WriteAsync(
@$"
        <script type=""application/json"" id=""helveg-icons-{iconSet.Namespace}"" class=""helveg-icons"">
            {JsonSerializer.Serialize(iconSet, HelvegDefaults.JsonOptions)}
        </script>
");
        }

        foreach (var script in Scripts)
        {
            await writer.WriteAsync(
@$"
        <script type=""text/javascript"">
            {script}
        </script>
");
        }

        writer.Write(
@"    </body>
</html>
");
    }

    private static async Task<string> GetBaseResource(string name)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Stream? stream = null;
        if (dir is not null)
        {
            var namePath = Path.Combine(dir, name);
            if (File.Exists(namePath))
            {
                stream = File.OpenRead(namePath);
            }
        }
        
        if (stream is null)
        {
            stream = typeof(UIBuilder).Assembly.GetManifestResourceStream(name);
        }

        if (stream is null)
        {
            throw new ArgumentException($"Could not find resource '{name}'.");
        }

        using (stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
