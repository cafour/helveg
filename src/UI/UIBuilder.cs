using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Helveg.Visualization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Helveg.UI;

public record UIScript(
    string FileName,
    string Contents,
    string Type = "text/javascript",
    string? Classes = null
);

public record UIStyle(
    string FileName,
    string Contents,
    string? Classes = null
);

public record InitializerOptions(
    string? MainRelation = null,
    string IconSetSelector = ".helveg-icons",
    string DataId = "helveg-data",
    ImmutableArray<string>? SelectedRelations = null,
    ImmutableArray<string>? SelectedKinds = null,
    int? ExpandedDepth = null
);

public class UIBuilder
{
    public const string DefaultEntryPointName = "index.html";
    public const string DefaultStylesDirName = "styles";
    public const string DefaultScriptsDirName = "scripts";

    public const string FaviconDataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAACxAAAAsQHGLUmNAAACgElEQVRYhcVXy23bQBCdBCmAufBq+U4gTAV2B3YA3i1VELsCOhUorsDSnUDkCkRXEAZgATrzpFQQY5C3wmA4s/zYgN9Fu8vdnTf/FU1FlWa3VZpdTj7o4OOMMw0RPVZplrwXAcaCiEr6b5Hla8jMJcAIrqiJaD+XxGsIMNZF1x4w3s+54MPYjdCQNT4qYSsiusG3TdG1qykERlmgSrMcQhsIkijFmONh+aYEoPkvFg5zf1FbOCBzMV9XabZ4MwLQnC98wtyqAYkar8cSiMZAlWb3wsTnEP448u5zEaAuXAvA9N/F0lH5ewiluGvtpWnMBUtl2uCKsbgW+zhGbqcSuFDzXM2PA0QSZE/AzVQCsYbDvt0JIp6v5R0LRcgngDSS5g/aBkEyK36gPlgWCSkbBPeU8iygfc2Xbwwh34joDP62gmyBwhS+9faM7QXBIpJAjnkIrsY4l6uaoOOqT4AfHI42eo2FX2F8cOIgce7yCYClZF3Dz1ulZS18ewAhLxbcjLEI/FExcImiwhXwL/x+LLq2UXu2IBo0rvG7EWs9Ip88Zg5KtNzPzvcggFP0GZkSiF5DuUELDGEpHqXPhnD+fSi69icyZG9U1SiBwQYifB+KEcElDZpWU6XZb6P81mo+m8CFEHrKgKJrgxX2Ruk27+4RKLq2x9KArGg7BGbooK5wqz17MbBz1gOS8OopuvZOZETpCCfL/DECT876CVobNBqz5QLbKQR2A+3WKruxZ9jBc61JAMH0ELtQTtBwYu3bfaq7daDo2nvPb0ZBiVlrFQvsoULE0c2lVEO7wHIJk2Lh1vkTRv0zQuXjJ1Xo+19VL+A9/zBkbTmIuWTHn21E9AKU08qnEZw5hQAAAABJRU5ErkJggg==";

    private readonly ILogger<UIBuilder> logger;

    private readonly HashSet<string> styleNames = new();
    private readonly HashSet<string> scriptNames = new();
    private readonly List<UIStyle> styles = new();
    private readonly List<UIScript> scripts = new();

    public DataModel Model { get; set; } = DataModel.CreateEmpty();
    public string Name { get; set; }
    public UIMode Mode { get; set; }
    public DirectoryInfo OutDir { get; set; }
    public InitializerOptions InitializerOptions { get; set; } = new();

    // Scripts and styles are put into separate directories not only because that's more readable
    // but also because that way name collisions do not matter.
    public string StylesDirName { get; set; } = DefaultStylesDirName;
    public string ScriptsDirName { get; set; } = DefaultScriptsDirName;
    public IReadOnlyList<UIStyle> Styles => styles;
    public IReadOnlyList<UIScript> Scripts => scripts;

    private UIBuilder(ILogger<UIBuilder>? logger = null)
    {
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<UIBuilder>();
        Name = $"helveg_{DateTimeOffset.Now:yyyy-MM-dd_HH-mm)}";
        OutDir = new DirectoryInfo(Environment.CurrentDirectory);
    }

    public static UIBuilder CreateDefault(ILogger<UIBuilder>? logger = null)
    {
        return new UIBuilder(logger)
            .AddStyle(UIConst.ExplorerCssResourceName, UIConst.ExplorerCss.Value)
            .AddScript(UIConst.DiagramJsResourceName, UIConst.DiagramJs.Value)
            .AddScript(UIConst.ExplorerJsResourceName, UIConst.ExplorerJs.Value)
            .AddScript(
                UIConst.VsIconSetResourceName,
                UIConst.VsIconSet.Value,
                "application/json",
                "helveg-icons")
            .AddScript(
                UIConst.PizzaIconSetResourceName,
                UIConst.PizzaIconSet.Value,
                "application/json",
                "helveg-icons")
            .AddScript(
                UIConst.NugetIconSetResourceName,
                UIConst.NugetIconSet.Value,
                "application/json",
                "helveg-icons");
    }

    public UIBuilder AddStyle(UIStyle style)
    {
        if (!styleNames.Add(style.FileName))
        {
            throw new ArgumentException($"Style with name '{style.FileName}' has already been added.");
        }

        styles.Add(style);
        logger.LogDebug("Added style '{}' with length '{}'.", style.FileName, style.Contents.Length);
        return this;
    }

    public UIBuilder AddStyle(
        string fileName,
        string contents,
        string? classes = null)
    {
        return AddStyle(new UIStyle(
            fileName,
            contents,
            classes
        ));
    }

    public UIBuilder AddScript(UIScript script)
    {
        if (!scriptNames.Add(script.FileName))
        {
            throw new ArgumentException($"Script with name '{script.FileName}' has already been added.");
        }

        scripts.Add(script);
        logger.LogDebug("Added script '{}' with length '{}'.", script.FileName, script.Contents.Length);
        return this;
    }

    public UIBuilder AddScript(
        string fileName,
        string contents,
        string type = "text/javascript",
        string? classes = null)
    {
        return AddScript(new UIScript(
            fileName,
            contents,
            type,
            classes
        ));
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

    public UIBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public UIBuilder SetMode(UIMode mode)
    {
        Mode = mode;
        return this;
    }

    public UIBuilder SetInitializerOptions(InitializerOptions options)
    {
        InitializerOptions = options;
        return this;
    }

    public UIBuilder SetOutDir(DirectoryInfo outDir)
    {
        OutDir = outDir;
        return this;
    }

    public async Task Build()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("The entry point name must be set.");
        }

        if (!OutDir.Exists)
        {
            OutDir.Create();
        }

        switch (Mode)
        {
            case UIMode.SingleFile:
                await BuildSingleFile();
                break;
            case UIMode.StaticApp:
                await BuildStatic();
                break;
            case UIMode.DataOnly:
                await BuildDataOnly();
                break;
            case UIMode.None:
                logger.LogInformation("Outputting nothing.");
                break;
            default:
                throw new InvalidOperationException($"'{Mode}' is not supported.");
        }

    }

    private async Task BuildSingleFile()
    {
        logger.LogInformation("Building '{}' as a single-file app at '{}.html'.", Model.Name, Name);
        var entryPoint = BuildEntryPoint(true);
        await File.WriteAllTextAsync(Path.Combine(OutDir.FullName, $"{Name}.html"), entryPoint);
    }

    private async Task BuildStatic()
    {
        logger.LogInformation("Building a '{}' static app at '{}'.", Model.Name, OutDir);

        async Task WriteFile(string filePath, string contents)
        {
            await File.WriteAllTextAsync(Path.Combine(OutDir.FullName, filePath), contents);
        }

        Task WriteJson<T>(string filePath, T value)
        {
            var contents = JsonSerializer.Serialize(value, HelvegDefaults.JsonOptions);
            return WriteFile(filePath, contents);
        }

        Directory.CreateDirectory(Path.Combine(OutDir.FullName, StylesDirName));
        foreach (var style in styles)
        {
            await WriteFile(Path.Combine(StylesDirName, style.FileName), style.Contents);
        }

        Directory.CreateDirectory(Path.Combine(OutDir.FullName, ScriptsDirName));
        foreach (var script in scripts)
        {
            await WriteFile(Path.Combine(ScriptsDirName, script.FileName), script.Contents);
        }

        await WriteJson($"{Name}-data.json", Model);

        await WriteFile(DefaultEntryPointName, BuildEntryPoint(false));
    }

    private async Task BuildDataOnly()
    {
        logger.LogInformation("Outputting raw data to '{}-data.json'.", Name);
        using var stream = new FileStream(
            Path.Combine(OutDir.FullName, $"{Name}-data.json"),
            FileMode.Create,
            FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, Model, HelvegDefaults.JsonOptions);
    }

    private string BuildEntryPoint(bool embedAll)
    {
        var sb = new StringBuilder();

        sb.Append(
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
            var classAttribute = style.Classes is not null ? $" class=\"{style.Classes}\"" : "";

            if (embedAll)
            {
                sb.Append(
@$"
        <!-- {style.FileName} -->
        <style{classAttribute}>
            {style.Contents}
        </style>
");
            }
            else
            {
                sb.Append(
$@"
        <link rel=""stylesheet"" type=""text/css"" href=""{StylesDirName}/{style.FileName}""{classAttribute}>");
            }
        }

        sb.Append(
@$"
    </head>

    <body>
        <div id=""helveg""></div>
        <!-- {Name} -->
");
        if (embedAll)
        {
            sb.Append(
$@"
        <!-- {Name}-data.json -->
        <script type=""application/json"" id=""helveg-data"">
            {JsonSerializer.Serialize(Model, HelvegDefaults.JsonOptions)}
        </script>
");
        }
        else
        {
            sb.Append(
$@"
        <script type=""application/json"" id=""helveg-data"" src=""{$"{Name}-data.json"}""></script>");
        }


        foreach (var script in scripts)
        {

            var classAttribute = script.Classes is not null ? $" class=\"{script.Classes}\"" : "";
            if (embedAll)
            {

                sb.Append(
@$"
        <!-- {script.FileName} -->
        <script type=""{script.Type}""{classAttribute}>
            {script.Contents}
        </script>
");
            }
            else
            {
                sb.Append(
@$"
        <script type=""{script.Type}"" src=""{ScriptsDirName}/{script.FileName}""{classAttribute}></script>");
            }
        }

        sb.Append(GetInitializer(InitializerOptions));

        sb.Append(
@"    </body>
</html>
");
        return sb.ToString();
    }

    private static string GetInitializer(InitializerOptions options)
    {
        dynamic refreshOptions = new ExpandoObject();
        if (options.ExpandedDepth is not null)
        {
            refreshOptions.expandedDepth = options.ExpandedDepth;
        }
        if (options.SelectedRelations is not null)
        {
            refreshOptions.selectedRelations = options.SelectedRelations;
        }
        if (options.SelectedKinds is not null)
        {
            refreshOptions.selectedKinds = options.SelectedKinds;
        }

        return
$@"<script type=""module"">
    const iconSets = await helveg.loadIconSets(""{options.IconSetSelector}"");
    const model = await helveg.loadModel(document.getElementById(""{options.DataId}""));
    const diagram = helveg.createDiagram({{
        iconSets: iconSets,
        model: model,
        mainRelation: {(options.MainRelation is null ? "null" : $"\"{options.MainRelation}\"")}
    }});
    helveg.createExplorer(diagram);
    await diagram.refresh({JsonSerializer.Serialize(refreshOptions, HelvegDefaults.JsonOptions)});

    // NB: this is left here mostly for debugging purposes
    helveg.diagram = diagram;
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
}
