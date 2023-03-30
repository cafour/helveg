using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Microsoft.Build.Locator;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.CommandLine.NamingConventionBinder;
using Helveg.Visualization;
using Helveg.CSharp;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading;
using Helveg.UI;

namespace Helveg.CommandLine;

public class Program
{
    public const string VerboseAlias = "--verbose";
    public const string ForceAlias = "--force";

    private ILoggerFactory logging = new NullLoggerFactory();
    private bool isForced = false;

    public async Task<Multigraph?> RunAnalysis(
        FileSystemInfo source,
        IDictionary<string, string> properties)
    {
        var logger = logging.CreateLogger("Analysis");
        FileInfo? file = source switch
        {
            FileInfo f => f.Extension == ".sln" || f.Extension == ".csproj" ? f : null,
            DirectoryInfo d => d.EnumerateFiles("*.sln").FirstOrDefault()
                ?? d.EnumerateFiles("*.csproj").FirstOrDefault(),
            _ => throw new NotImplementedException() // this should never happen
        };

        if (file is null)
        {
            logger.LogCritical($"No solution or project could be located in the '{source}' directory.");
            return null;
        }

        logger.LogDebug($"Found '{file.FullName}'.");

        // var cache = IsForced ? null : await Serialize.GetCache<SerializableSolution>(AnalysisCacheFilename, logger);
        var vsInstance = MSBuildLocator.RegisterDefaults();
        logger.LogDebug($"Using MSBuild at '{vsInstance.MSBuildPath}'.");

        var options = new EntityWorkspaceAnalysisOptions
        {
            MSBuildProperties = properties.ToImmutableDictionary()
        };
        var workspaceProvider = new DefaultEntityWorkspaceProvider(
            logging.CreateLogger<DefaultEntityWorkspaceProvider>());
        var workspace = await workspaceProvider.GetWorkspace(file.FullName, options);
        using var analysisStream = new FileStream("analysis.json", FileMode.Create, FileAccess.ReadWrite);
        await JsonSerializer.SerializeAsync(analysisStream, workspace, HelvegDefaults.JsonOptions);

        var visualizationVisitor = new VisualizationEntityVisitor();
        visualizationVisitor.Visit(workspace);
        return visualizationVisitor.Build();
    }

    public async Task<int> RunPipeline(FileSystemInfo source, Dictionary<string, string> properties)
    {
        // code analysis
        var multigraph = await RunAnalysis(source, properties);
        if (multigraph is null)
        {
            return 1;
        }

        var vmb = VisualizationModelBuilder.CreateDefault();
        vmb.SetDocumentInfo(new DocumentInfo(
            Name: multigraph.Label ?? multigraph.Id,
            CreatedOn: DateTimeOffset.UtcNow,
            HelvegVersion: GitVersionInformation.FullSemVer,
            Revision: null));
        vmb.SetMultigraph(multigraph);
        vmb.UseCSharp();

        var sfb = await SingleFileBuilder.CreateDefault();
        sfb.SetVisualizationModel(vmb.Build());

        using var fileStream = new FileStream("output.html", FileMode.Create, FileAccess.ReadWrite);
        using var writer = new StreamWriter(fileStream);
        await sfb.Build(writer);
        return 0;
    }

    public static async Task<int> Main(string[] args)
    {
        var program = new Program();
        var rootCmd = new RootCommand("A software visualization tool")
        {
            Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(RunPipeline))!, program)
        };
        var verboseOption = new Option<bool>(new[] { "-v", VerboseAlias }, "Set logging level to Debug");
        var forceOption = new Option<bool>(new[] { "-f", ForceAlias }, "Overwrite cached results");
        rootCmd.AddGlobalOption(verboseOption);
        rootCmd.AddGlobalOption(forceOption);
        rootCmd.AddArgument(new Argument<FileSystemInfo>(
            name: "SOURCE",
            description: "Path to a project or a solution",
            getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory)));
        var propertyOption = new Option<Dictionary<string, string>>(
            aliases: new[] { "-p", "--properties" },
            parseArgument: a => a.Tokens
                .Select(t => t.Value.Split(new[] { '=' }, 2))
                .ToDictionary(p => p[0], p => p[1]),
            description: "Set an MSBuild property for the loading of projects");
        propertyOption.AddValidator(r =>
        {
            foreach (var token in r.Tokens)
            {
                if (!token.Value.Contains('='))
                {
                    r.ErrorMessage = "MSBuild properties must follow the '<n>=<v>' format.";
                }
            }
        });
        rootCmd.AddOption(propertyOption);

        var builder = new CommandLineBuilder(rootCmd)
            .UseHelp()
            .AddMiddleware(c =>
            {
                program.isForced = c.ParseResult.GetValueForOption<bool>(forceOption);

                LogLevel minimumLevel = c.ParseResult.GetValueForOption<bool>(verboseOption)
                    ? LogLevel.Debug
                    : LogLevel.Information;
                program.logging = LoggerFactory.Create(b =>
                {
                    b.AddConsole();
                    b.SetMinimumLevel(minimumLevel);
                });
            })
            .Build(); // Sets ImplicitParser inside the root command. Yes, it's weird, I know.
        var errorCode = await builder.InvokeAsync(args);
        program.logging.Dispose();
        return errorCode;
    }
}
