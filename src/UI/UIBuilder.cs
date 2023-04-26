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

    private readonly ILogger<UIBuilder> logger;

    private readonly HashSet<string> styleNames = new();
    private readonly HashSet<string> scriptNames = new();
    private readonly HashSet<string> iconSetNamespaces = new();
    private readonly List<(string fileName, string contents)> styles = new();
    private readonly List<(string fileName, string contents)> scripts = new();
    private readonly List<IconSet> iconSets = new();

    public VisualizationModel Model { get; set; } = VisualizationModel.Invalid;
    public string EntryPointName { get; set; } = DefaultEntryPointName;
    public string? DataDirectory { get; set; }
    public string IconsDirectory { get; set; } = DefaultIconsDirectory;
    public string StylesDirectory { get; set; } = DefaultStylesDirectory;
    public string ScriptsDirectory { get; set; } = DefaultScriptsDirectory;
    public UIMode Mode { get; set; } = UIMode.SingleFile;

    public IReadOnlyList<(string fileName, string contents)> Styles => styles;
    public IReadOnlyList<(string fileName, string contents)> Scripts => scripts;
    public IReadOnlyList<IconSet> IconSets => iconSets;

    private UIBuilder(ILogger<UIBuilder>? logger = null)
    {
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<UIBuilder>();
    }

    public static async Task<UIBuilder> CreateDefault(ILogger<UIBuilder>? logger = null)
    {
        return new UIBuilder(logger)
            .AddStyle(UIConst.CssResourceName, await GetBaseResource(UIConst.CssResourceName))
            .AddScript(UIConst.JsResourceName, await GetBaseResource(UIConst.JsResourceName))
            .AddIconSet(await IconSet.LoadFromAssembly(UIConst.BaseIconNamespace, typeof(Icon).Assembly));
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

    public UIBuilder AddIconSet(IconSet iconSet)
    {
        if (string.IsNullOrWhiteSpace(iconSet.Namespace))
        {
            throw new ArgumentNullException(nameof(iconSet.Namespace));
        }

        if (!iconSetNamespaces.Add(iconSet.Namespace))
        {
            throw new ArgumentException($"IconSet with namespace '{iconSet.Namespace}' has already been added.");
        }

        iconSets.Add(iconSet);
        logger.LogDebug("Added icon set '{}' with {} icons.", iconSet.Namespace, iconSet.Icons.Count);
        return this;
    }

    public UIBuilder SetVisualizationModel(VisualizationModel visualizationModel)
    {
        Model = visualizationModel;
        logger.LogDebug(
            "Using the '{}' visualization model with {} nodes, {} relations, and {} edges in total.",
            Model.Name, 
            Model.Multigraph.Nodes.Count, 
            Model.Multigraph.Relations.Count, 
            Model.Multigraph.Relations.Sum(r => r.Value.Edges.Length));
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
        logger.LogInformation("Building '{}' as a single-file app.", Model.Name);

        using var stream = streamFactory(EntryPointName);
        using var writer = new StreamWriter(stream);

        writer.Write(
@$"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <title>{Model.Name} | Helveg</title>
        <meta charset=""utf-8"" />
        <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
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
        <div id=""app""></div>
        <script type=""application/json"" id=""helveg-data"">
            {JsonSerializer.Serialize(Model, HelvegDefaults.JsonOptions)}
        </script>
");

        foreach (var iconSet in IconSets)
        {
            await writer.WriteAsync(
@$"
        <!-- IconSet with namespace '{iconSet.Namespace}' -->
        <script type=""application/json"" class=""helveg-icons"">
            {JsonSerializer.Serialize(iconSet, HelvegDefaults.JsonOptions)}
        </script>
");
        }

        foreach (var script in scripts)
        {
            await writer.WriteAsync(
@$"
        <!-- {script.fileName} -->
        <script type=""text/javascript"">
            {script.contents}
        </script>
");
        }

        writer.Write(
@"    </body>
</html>
");
    }

    private async Task BuildStatic(Func<string, Stream> streamFactory)
    {
        logger.LogInformation("Building a '{}' static app.", Model.Name);

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

        var iconSetPaths = new List<string>();
        foreach (var iconSet in iconSets)
        {
            var iconSetPath = Path.Combine(IconsDirectory, GetIconSetFileName(iconSet.Namespace));
            await WriteJson(iconSetPath, iconSet);
            iconSetPaths.Add(NormalizePath(iconSetPath));
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
        <title>{Model.DocumentInfo.Name ?? "Unknown"} | Helveg</title>
        <meta charset=""utf-8"" />
        <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
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

        foreach (var iconSetPath in iconSetPaths)
        {
            entryPointWriter.Write(
@$"
        <script type=""application/json"" class=""helveg-icons"" src=""{iconSetPath}""></script>
");
        }

        foreach (var scriptPath in scriptPaths)
        {
            entryPointWriter.Write(
@$"
        <script type=""text/javascript"" src=""{scriptPath}""></script>
");
        }

        entryPointWriter.Write(
@"    </body>
</html>
");
    }

    private static string GetIconSetFileName(string @namespace)
    {
        return $"icons-{@namespace}.json";
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
