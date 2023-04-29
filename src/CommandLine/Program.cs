using System.CommandLine;
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
using System.CommandLine.NamingConventionBinder;
using Helveg.Visualization;
using Helveg.CSharp;
using System.Collections.Immutable;
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
        IDictionary<string, string> properties,
        AnalysisScope projectAnalysis,
        AnalysisScope externalAnalysis)
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
                    MSBuildProperties = msbuildProperties,
                    IncludeExternalDependencies = externalAnalysis >= AnalysisScope.WithoutSymbols
                },
                logger: logging.CreateLogger<MSBuildMiner>())
            .AddRoslyn(
                options: new RoslynMinerOptions
                {
                    MSBuildProperties = msbuildProperties,
                    ProjectSymbolAnalysisScope = ToSymbolAnalysisScope(projectAnalysis),
                    ExternalSymbolAnalysisScope = ToSymbolAnalysisScope(externalAnalysis)
                },
                logger: logging.CreateLogger<RoslynMiner>());

        var workspace = await workflow.Run(new DataSource(source.FullName, DateTimeOffset.UtcNow));

        var multigraphBuilder = new MultigraphBuilder();

        var symbolVisitor = new VisualizationSymbolVisitor(multigraphBuilder);
        workspace.Accept(symbolVisitor);

        var projectVisitor = new VisualizationProjectVisitor(multigraphBuilder);
        workspace.Accept(projectVisitor);

        return multigraphBuilder.Build();
    }

    public async Task<int> RunPipeline(
        FileSystemInfo source,
        Dictionary<string, string> properties,
        AnalysisScope projectAnalysis,
        AnalysisScope externalAnalysis,
        UIMode mode,
        string outDir,
        string styleDir,
        string scriptDir,
        string iconDir)
    {
        var outDirInfo = new DirectoryInfo(outDir);
        if (!outDirInfo.Exists)
        {
            outDirInfo.Create();
        }

        // code analysis
        var multigraph = await RunAnalysis(source, properties, projectAnalysis, externalAnalysis);
        if (multigraph is null)
        {
            return 1;
        }

        var uib = await UIBuilder.CreateDefault(logging.CreateLogger<UIBuilder>());
        await uib.UseCSharp();
        uib.Mode = mode;
        uib.StylesDirectory = styleDir;
        uib.ScriptsDirectory = scriptDir;
        uib.IconsDirectory = iconDir;
        uib.SetVisualizationModel(new()
        {
            DocumentInfo = new()
            {
                Name = Path.GetFileNameWithoutExtension(source.FullName) ?? multigraph.Id,
                CreatedOn = DateTimeOffset.UtcNow
            },
            Multigraph = multigraph
        });

        await uib.Build(path =>
        {
            var fullPath = Path.Combine(outDirInfo.FullName, path);
            var dirName = Path.GetDirectoryName(fullPath);
            if (dirName is not null && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            return new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite);
        });
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
            getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory))
        {
            Arity = ArgumentArity.ExactlyOne
        });
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
        rootCmd.AddOption(new Option<AnalysisScope>(
            aliases: new[] { "-pa", "--project-analysis" },
            getDefaultValue: () => AnalysisScope.All,
            description: "Scope of the project analysis"));
        rootCmd.AddOption(new Option<AnalysisScope>(
            aliases: new[] { "-ea", "--external-analysis" },
            getDefaultValue: () => AnalysisScope.PublicApi,
            description: "Scope of analysis of external dependencies"));
        rootCmd.AddOption(new Option<UIMode>(
            aliases: new[] { "-m", "--mode" },
            getDefaultValue: () => UIMode.SingleFile,
            description: "UI mode to use."));
        rootCmd.AddOption(new Option<string>(
            aliases: new[] { "--outdir" },
            getDefaultValue: () => Directory.GetCurrentDirectory(),
            description: "Output directory"));
        rootCmd.AddOption(new Option<string>(
            aliases: new[] { "--styledir" },
            getDefaultValue: () => "styles",
            description: "Output subdrectory for CSS stylesheets"));
        rootCmd.AddOption(new Option<string>(
            aliases: new[] { "--scriptdir" },
            getDefaultValue: () => "scripts",
            description: "Output subdirectory for JS scripts"));
        rootCmd.AddOption(new Option<string>(
            aliases: new[] { "--icondir" },
            getDefaultValue: () => "icons",
            description: "Output subdirectory for IconSet files"));

        var builder = new CommandLineBuilder(rootCmd)
            .UseHelp()
            .UseExceptionHandler((e, c) =>
            {
                program.logging.CreateLogger("").LogCritical(e, e.Message);
            }, 1)
            .AddMiddleware(c =>
            {
                bool isVerbose = c.ParseResult.GetValueForOption(verboseOption);
                
                program.isForced = c.ParseResult.GetValueForOption(forceOption);

                LogLevel minimumLevel = isVerbose ? LogLevel.Debug : LogLevel.Information;
                program.logging = LoggerFactory.Create(b =>
                {
                    b.AddConsoleFormatter<BriefConsoleFormatter, BriefConsoleFormatterOptions>(d =>
                    {
                        d.TimestampFormat = "HH:mm:ss.fff";
                        d.IncludeStacktraces = isVerbose;
                    });
                    b.AddConsole(d => d.FormatterName = "brief");
                    b.SetMinimumLevel(minimumLevel);
                });
            })
            .Build(); // Sets ImplicitParser inside the root command. Yes, it's weird, I know.
        var errorCode = await builder.InvokeAsync(args);
        program.logging.Dispose();
        return errorCode;
    }

    private static SymbolAnalysisScope ToSymbolAnalysisScope(AnalysisScope scope)
    {
        return scope switch
        {
            AnalysisScope.None => SymbolAnalysisScope.None,
            AnalysisScope.PublicApi => SymbolAnalysisScope.PublicApi,
            AnalysisScope.Explicit => SymbolAnalysisScope.Explicit,
            AnalysisScope.All => SymbolAnalysisScope.All,
            _ => SymbolAnalysisScope.None
        };
    }

}
