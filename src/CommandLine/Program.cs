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
using Helveg.CSharp.Packages;
using Microsoft.Extensions.Options;

namespace Helveg.CommandLine;

public static class Program
{
    private static ILoggerFactory loggerFactory = null!;
    private static ILogger logger = null!;

    public static async Task<int> Main(string[] args)
    {
        var rootCmd = new RootCommand("An extensible software visualization tool")
        {
            Config.SourceArg,
            Config.PresetOpt,
            Config.CompareToOpt,
            Config.ProjectAnalysisOpt,
            Config.ExternalAnalysisOpt,
            Config.ModeOpt,
            Config.DryRunOpt,
            Config.NameOpt,
            Config.OutDirOpt,
            Config.BuildPropertiesOpt,
            Config.ForceOpt,
            Config.InitialDepthOpt,
            Config.InitialRelationsOpt,
            Config.InitialKindsOpt
        };
        var verboseOpt = new Option<bool>(new[] { "-v", "--verbose" }, "Set logging level to Debug.");
        rootCmd.AddGlobalOption(verboseOpt);
        var versionOpt = new Option<bool>(new[] { "--version" }, "Print Helveg's version.");
        rootCmd.AddGlobalOption(versionOpt);

        loggerFactory = new NullLoggerFactory();
        logger = loggerFactory.CreateLogger("");

        rootCmd.SetHandler(Run, new Config.Binder());

        var cmdBuilder = new CommandLineBuilder(rootCmd)
            .UseHelp()
            .AddMiddleware(c =>
            {
                if (c.ParseResult.GetValueForOption(versionOpt))
                {
                    Console.WriteLine(GitVersionInformation.SemVer);
                    Environment.Exit(0);
                    return;
                }

                bool isVerbose = c.ParseResult.GetValueForOption(verboseOpt);
                var minimumLevel = isVerbose ? LogLevel.Debug : LogLevel.Information;
                loggerFactory = LoggerFactory.Create(b =>
                {
                    b.AddConsoleFormatter<BriefConsoleFormatter, BriefConsoleFormatterOptions>(d =>
                    {
                        d.TimestampFormat = "HH:mm:ss.fff";
                        d.IncludeStacktraces = isVerbose;
                    });
                    b.AddConsole(d => d.FormatterName = "brief");
                    b.SetMinimumLevel(minimumLevel);
                });
                logger = loggerFactory.CreateLogger("");
            })
            .UseExceptionHandler((e, c) =>
            {
                logger.LogCritical(e, e.Message);
            }, 1)
            .Build(); // Sets ImplicitParser inside the root command. Yes, it's weird, I know.

        var errorCode = await cmdBuilder.InvokeAsync(args);
        loggerFactory.Dispose();
        return errorCode;
    }

    public static async Task<int> Run(Config config)
    {
        foreach (var property in config.BuildProperties)
        {
            logger.LogInformation($"{property.Key}={property.Value}");
        }

        if (config.OutDir is null)
        {
            logger.LogCritical("'{}' is not a valid valid output directory.", config.OutDir);
            return 1;
        }

        if (!config.OutDir.Exists)
        {
            config.OutDir.Create();
        }

        // code analysis
        var model = await Analyze(config);

        // output
        await UIBuilder.CreateDefault(loggerFactory.CreateLogger<UIBuilder>())
            .SetMode(config.Mode)
            .SetName(config.Name ?? model.Name)
            .SetInitializerOptions(new(
                MainRelation: CSRelations.Declares,
                SelectedRelations: config.InitialRelations,
                SelectedKinds: config.InitialKinds,
                ExpandedDepth: config.InitialDepth
            ))
            .SetDataModel(model)
            .SetOutDir(config.OutDir)
            .Build();
        return 0;
    }

    public static async Task<DataModel> Analyze(Config config)
    {
        if (config.Source is null || !config.Source.Exists)
        {
            logger.LogCritical("'{}' is not a valid SOURCE argument.", config.Source);
            return DataModel.CreateInvalid(
                diagnostics: new[] { new MultigraphDiagnostic() {
                    Message = "Invalid SOURCE argument.",
                    Severity = MultigraphDiagnosticSeverity.Error
                }}
            );
        }

        var msbuildInstance = MSBuildLocator.RegisterDefaults();
        logger.LogDebug("Using MSBuild at '{}'.", msbuildInstance.MSBuildPath);

        var msbuildProperties = config.BuildProperties.ToImmutableDictionary();
        var workflow = new Workflow()
            .AddMSBuild(
                options: new MSBuildMinerOptions
                {
                    MSBuildProperties = msbuildProperties,
                    IncludeExternalDependencies = config.ExternalAnalysis >= AnalysisScope.WithoutSymbols
                },
                logger: loggerFactory.CreateLogger<MSBuildMiner>());

        if (config.ExternalAnalysis > AnalysisScope.None)
        {
            workflow.AddNuGet(
                logger: loggerFactory.CreateLogger<NuGetMiner>());
        }

        workflow.AddRoslyn(
            options: new RoslynMinerOptions
            {
                MSBuildProperties = msbuildProperties,
                ProjectSymbolAnalysisScope = ToSymbolAnalysisScope(config.ProjectAnalysis),
                ExternalSymbolAnalysisScope = ToSymbolAnalysisScope(config.ExternalAnalysis)
            },
            logger: loggerFactory.CreateLogger<RoslynMiner>());

        var workspace = await workflow.Run(new DataSource(config.Source.FullName, DateTimeOffset.UtcNow));

        var graph = new Multigraph()
        {
            Nodes = new(),
            Relations = new()
        };

        var projectVisitor = new VisualizationProjectVisitor(graph);
        workspace.Accept(projectVisitor);

        var packageVisitor = new VisualizationPackageVisitor(graph);
        workspace.Accept(packageVisitor);

        var symbolVisitor = new VisualizationSymbolVisitor(graph);
        workspace.Accept(symbolVisitor);

        var now = DateTimeOffset.UtcNow;

        return new()
        {
            Name = config.Name
                ?? $"{config.Source.Name}_{now:yyyy-MM-dd_HH-mm-ss}",
            CreatedOn = now,
            Analyzer = new()
            {
                Name = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                Version = GitVersionInformation.SemVer
            },
            Data = graph
        };
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
