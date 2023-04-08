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
using Helveg.CSharp.Symbols;
using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging.Console;

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
        var logger = logging.CreateLogger<Program>();

        if (!source.Exists)
        {
            logger.LogCritical("Source '{}' does not exist.", source.FullName);
            return null;
        }

        var msbuildInstance = MSBuildLocator.RegisterDefaults();
        logger.LogDebug("Using MSBuild at '{}'.", msbuildInstance.MSBuildPath);

        var msbuildProperties = properties.ToImmutableDictionary();
        var workflow = new Workflow()
            .AddMSBuild(
                options: new MSBuildMinerOptions
                {
                    MSBuildProperties = msbuildProperties
                },
                logger: logging.CreateLogger<MSBuildMiner>())
            .AddRoslyn(
                options: new RoslynMinerOptions
                {
                    MSBuildProperties = msbuildProperties
                },
                logger: logging.CreateLogger<RoslynMiner>());

        var workspace = await workflow.Run(new DataSource(source.FullName, DateTimeOffset.UtcNow));

        var multigraphBuilder = new MultigraphBuilder
        {
            Label = Path.GetFileNameWithoutExtension(workspace.Source.Path)
        };

        var symbolVisitor = new VisualizationSymbolVisitor(multigraphBuilder);
        workspace.Accept(symbolVisitor);

        var projectVisitor = new VisualizationProjectVisitor(multigraphBuilder);
        workspace.Accept(projectVisitor);

        return multigraphBuilder.Build();
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
        await sfb.UseCSharp();
        sfb.SetVisualizationModel(vmb.Build());

        using var fileStream = new FileStream("output.html", FileMode.Create, FileAccess.ReadWrite);
        using var writer = new StreamWriter(fileStream);
        await sfb.Build(writer);
        return 0;
    }

    public static async Task<int> Main(string[] args)
    {
        var program = new Program();
        var rootCmd = new RootCommand("An extensible software visualization tool")
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
                    b.AddConsoleFormatter<BriefConsoleFormatter, ConsoleFormatterOptions>(d => d.TimestampFormat = "HH:mm:ss.fff");
                    b.AddConsole(d => d.FormatterName = "brief");
                    b.SetMinimumLevel(minimumLevel);
                });
            })
            .Build(); // Sets ImplicitParser inside the root command. Yes, it's weird, I know.
        var errorCode = await builder.InvokeAsync(args);
        program.logging.Dispose();
        return errorCode;
    }
}
