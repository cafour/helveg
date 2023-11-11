using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Helveg.Visualization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Helveg.UI;

public class UIBuilder
{
    public const string DefaultEntryPointName = "index.html";
    public const string DefaultIconsDirectory = "icons";
    public const string DefaultStylesDirectory = "styles";
    public const string DefaultScriptsDirectory = "scripts";

    public const string FaviconDataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAACxAAAAsQHGLUmNAAACgElEQVRYhcVXy23bQBCdBCmAufBq+U4gTAV2B3YA3i1VELsCOhUorsDSnUDkCkRXEAZgATrzpFQQY5C3wmA4s/zYgN9Fu8vdnTf/FU1FlWa3VZpdTj7o4OOMMw0RPVZplrwXAcaCiEr6b5Hla8jMJcAIrqiJaD+XxGsIMNZF1x4w3s+54MPYjdCQNT4qYSsiusG3TdG1qykERlmgSrMcQhsIkijFmONh+aYEoPkvFg5zf1FbOCBzMV9XabZ4MwLQnC98wtyqAYkar8cSiMZAlWb3wsTnEP448u5zEaAuXAvA9N/F0lH5ewiluGvtpWnMBUtl2uCKsbgW+zhGbqcSuFDzXM2PA0QSZE/AzVQCsYbDvt0JIp6v5R0LRcgngDSS5g/aBkEyK36gPlgWCSkbBPeU8iygfc2Xbwwh34joDP62gmyBwhS+9faM7QXBIpJAjnkIrsY4l6uaoOOqT4AfHI42eo2FX2F8cOIgce7yCYClZF3Dz1ulZS18ewAhLxbcjLEI/FExcImiwhXwL/x+LLq2UXu2IBo0rvG7EWs9Ip88Zg5KtNzPzvcggFP0GZkSiF5DuUELDGEpHqXPhnD+fSi69icyZG9U1SiBwQYifB+KEcElDZpWU6XZb6P81mo+m8CFEHrKgKJrgxX2Ruk27+4RKLq2x9KArGg7BGbooK5wqz17MbBz1gOS8OopuvZOZETpCCfL/DECT876CVobNBqz5QLbKQR2A+3WKruxZ9jBc61JAMH0ELtQTtBwYu3bfaq7daDo2nvPb0ZBiVlrFQvsoULE0c2lVEO7wHIJk2Lh1vkTRv0zQuXjJ1Xo+19VL+A9/zBkbTmIuWTHn21E9AKU08qnEZw5hQAAAABJRU5ErkJggg==";


    private readonly ILogger<UIBuilder> logger;

    private readonly HashSet<string> styleNames = new();
    private readonly HashSet<string> scriptNames = new();
    private readonly HashSet<string> iconSetNamespaces = new();
    private readonly List<(string fileName, string contents)> styles = new();
    private readonly List<(string fileName, string contents)> scripts = new();
    private readonly List<string> pluginExpressions = new();

    public DataModel Model { get; set; } = UIConst.InvalidDataModel;
    public string EntryPointName { get; set; } = DefaultEntryPointName;
    public string? DataDirectory { get; set; }
    public string IconsDirectory { get; set; } = DefaultIconsDirectory;
    public string StylesDirectory { get; set; } = DefaultStylesDirectory;
    public string ScriptsDirectory { get; set; } = DefaultScriptsDirectory;
    public UIMode Mode { get; set; } = UIMode.SingleFile;

    public IReadOnlyList<(string fileName, string contents)> Styles => styles;
    public IReadOnlyList<(string fileName, string contents)> Scripts => scripts;

    private UIBuilder(ILogger<UIBuilder>? logger = null)
    {
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<UIBuilder>();
    }

    public static async Task<UIBuilder> CreateDefault(ILogger<UIBuilder>? logger = null)
    {
        return new UIBuilder(logger)
            .AddStyle(UIConst.ExplorerCssResourceName, await GetBaseResource(UIConst.ExplorerCssResourceName))
            .AddScript(UIConst.DiagramJsResourceName, await GetBaseResource(UIConst.DiagramJsResourceName))
            .AddScript(UIConst.ExplorerJsResourceName, await GetBaseResource(UIConst.ExplorerJsResourceName))
            .AddScript(UIConst.VsIconSetResourceName, await GetBaseResource(UIConst.VsIconSetResourceName))
            .AddScript(UIConst.PizzaIconSetResourceName, await GetBaseResource(UIConst.PizzaIconSetResourceName))
            .AddScript(UIConst.NugetIconSetResourceName, await GetBaseResource(UIConst.NugetIconSetResourceName));
    }

    public UIBuilder AddStyle(string fileName, string contents)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (!styleNames.Add(fileName))
        {
            throw new ArgumentException($"Style with name '{fileName}' has already been added.");
        }

        styles.Add((fileName, contents));
        logger.LogDebug("Added style '{}' with length '{}'.", fileName, contents.Length);
        return this;
    }

    public UIBuilder AddScript(string fileName, string contents)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (!scriptNames.Add(fileName))
        {
            throw new ArgumentException($"Script with name '{fileName}' has already been added.");
        }

        scripts.Add((fileName, contents));
        logger.LogDebug("Added script '{}' with length '{}'.", fileName, contents.Length);
        return this;
    }

    public UIBuilder SetDataModel(DataModel model)
    {
        Model = model;
        logger.LogDebug(
            "Using the '{}' visualization model with {} nodes, {} relations, and {} edges in total.",
            Model.Name,
            Model.Data?.Nodes.Count,
            Model.Data?.Relations.Count,
            Model.Data?.Relations.Sum(r => r.Value.Edges.Count));
        return this;
    }

    public UIBuilder SetEntryPointName(string entryPointName)
    {
        EntryPointName = entryPointName;
        return this;
    }

    public UIBuilder SetMode(UIMode mode)
    {
        Mode = mode;
        return this;
    }

    public UIBuilder AddPlugin(string expression)
    {
        pluginExpressions.Add(expression);
        return this;
    }

    public async Task Build(Func<string, Stream> streamFactory)
    {
        if (string.IsNullOrWhiteSpace(EntryPointName))
        {
            throw new ArgumentException("The entry point name must be set.");
        }

        switch (Mode)
        {
            case UIMode.SingleFile:
                await BuildSingleFile(streamFactory);
                break;
            case UIMode.StaticApp:
                await BuildStatic(streamFactory);
                break;
            default:
                throw new InvalidOperationException($"'{Mode}' is not a valid UIMode.");
        }

    }

    private async Task BuildSingleFile(Func<string, Stream> streamFactory)
    {
        logger.LogInformation("Building '{}' as a single-file app at '{}'.", Model.Name, EntryPointName);

        using var stream = streamFactory(EntryPointName);
        using var writer = new StreamWriter(stream);

        writer.Write(
@$"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <title>{Model.Name} | Helveg</title>
        <meta charset=""utf-8"" />
        <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
        <link rel=""icon"" type=""image/png"" href=""{FaviconDataUrl}"" />
        ");
        foreach (var style in styles)
        {
            await writer.WriteAsync(
@$"
        <!-- {style.fileName} -->
        <style>
            {style.contents}
        </style>
");
        }

        await writer.WriteAsync(
@$"
    </head>

    <body>
        <div id=""helveg""></div>
        <script type=""application/json"" id=""helveg-data"">
            {JsonSerializer.Serialize(Model, HelvegDefaults.JsonOptions)}
        </script>
");

        foreach (var script in scripts)
        {
            await writer.WriteAsync(
@$"
        <!-- {script.fileName} -->
        <script type=""{(Path.GetExtension(script.fileName) == ".js" ? "text/javascript" : "application/json")}"" {(Path.GetExtension(script.fileName) == ".json" ? "class=\"helveg-icons\"" : "")}>
            {script.contents}
        </script>
");
        }

        writer.Write(GetInitializer());

        writer.Write(
@"    </body>
</html>
");
    }

    private async Task BuildStatic(Func<string, Stream> streamFactory)
    {
        logger.LogInformation("Building a '{}' static app at '{}'.", Model.Name, EntryPointName);

        async Task WriteFile(string filePath, string contents)
        {
            using var stream = streamFactory(filePath);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(contents);
        }

        async Task WriteJson<T>(string filePath, T value)
        {
            using var stream = streamFactory(filePath);
            await JsonSerializer.SerializeAsync(stream, value, HelvegDefaults.JsonOptions);
        }

        var stylePaths = new List<string>();
        foreach (var (fileName, contents) in styles)
        {
            var stylePath = Path.Combine(StylesDirectory, fileName);
            await WriteFile(stylePath, contents);
            stylePaths.Add(NormalizePath(stylePath));
        }

        var scriptPaths = new List<string>();
        foreach (var (fileName, contents) in scripts)
        {
            var scriptPath = Path.Combine(ScriptsDirectory, fileName);
            await WriteFile(scriptPath, contents);
            scriptPaths.Add(NormalizePath(scriptPath));
        }

        var dataPath = !string.IsNullOrEmpty(DataDirectory)
            ? Path.Combine(DataDirectory, UIConst.DataFileName)
            : UIConst.DataFileName;
        dataPath = NormalizePath(dataPath);
        await WriteJson(dataPath, Model);

        using var entryPointStream = streamFactory(EntryPointName);
        using var entryPointWriter = new StreamWriter(entryPointStream);
        entryPointWriter.Write(
@$"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <title>{Model.Name ?? "Unknown"} | Helveg</title>
        <meta charset=""utf-8"" />
        <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
        <link rel=""icon"" type=""image/png"" href=""{FaviconDataUrl}"" />
        ");
        foreach (var stylePath in stylePaths)
        {
            entryPointWriter.Write(
@$"
        <link rel=""stylesheet"" type=""text/css"" href=""{stylePath}"">
");
        }

        entryPointWriter.Write(
@$"
    </head>

    <body>
        <div id=""app""></div>
        <script type=""application/json"" id=""helveg-data"" src=""{dataPath}""></script>
");

        foreach (var scriptPath in scriptPaths)
        {
            entryPointWriter.Write(
@$"
        <script type=""{(Path.GetExtension(scriptPath) == ".js" ? "text/javascript" : "application/json")}"" src=""{scriptPath}"" {(Path.GetExtension(scriptPath) == ".json" ? "class=\"helveg-icons\"" : "")}></script>
");
        }

        entryPointWriter.Write(GetInitializer());

        entryPointWriter.Write(
@"    </body>
</html>
");
    }

    private string GetInitializer()
    {
        return
@$"<script type=""module"">
    let iconSets = await helveg.loadIconSets("".helveg-icons"");
        let model = await helveg.loadModel(document.getElementById(""helveg-data""));
        window.diagram = helveg.createDiagram({{
            iconSets: iconSets,
            model: model,
            mainRelation: ""declares""
        }});
        await window.diagram.resetLayout();
        helveg.createExplorer(diagram);
</script>";
    }

    private static string NormalizePath(string path)
    {
        // TODO: A more sophisticated path normalization is probably needed.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return path.Replace('\\', '/');
        }
        return path;
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
