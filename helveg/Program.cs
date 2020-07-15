using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Helveg.Serialization;
using Helveg.Landscape;
using Helveg.Analysis;
using Helveg.Render;
using Microsoft.Build.Locator;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.CommandLine.Builder;
using System.Collections.Generic;

namespace Helveg
{
    public class Program
    {
        public const string AnalysisCacheFilename = "helveg-analysis.json";
        public const string FdgCacheFilename = "helveg-fdg.json";
        public const int RegularIterationCount = 2000;
        public const int NoOverlapIterationCount = 2000;
        public const int StrongGravityIterationCount = 1000;
        public const string VkDebugAlias = "--vk-debug";
        public const string VerboseAlias = "--verbose";
        public const string ForceAlias = "--force";

        public static ILoggerFactory Logging = new NullLoggerFactory();
        public static bool IsForced = false;

        public static async Task<AnalyzedSolution?> RunAnalysis(
            FileSystemInfo source,
            IDictionary<string, string> properties)
        {
            var logger = Logging.CreateLogger("Analysis");
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

            var cache = IsForced ? null : await Serialize.GetCache<SerializableSolution>(AnalysisCacheFilename, logger);
            var vsInstance = MSBuildLocator.RegisterDefaults();
            logger.LogDebug($"Using MSBuild at '{vsInstance.MSBuildPath}'.");

            var analyzedSolution = await Analyze.AnalyzeProjectOrSolution(file, properties, logger, cache);
            if (analyzedSolution is object)
            {
                await Serialize.SetCache(
                    AnalysisCacheFilename,
                    SerializableSolution.FromAnalyzed(analyzedSolution.Value),
                    logger);
            }
            return analyzedSolution;
        }

        public static Graph RunFdg(AnalyzedProject project, SerializableGraphCollection? cache = null)
        {
            var logger = Logging.CreateLogger($"{project.Name}: Fdg");
            if (cache is object
                && cache.Graphs is object
                && cache.Graphs.TryGetValue(project.Name, out var cachedGraph)
                && cachedGraph.TimeStamp == project.LastWriteTime)
            {
                logger.LogInformation("Using cached Fdg results.");
                return cachedGraph.ToGraph();
            }

            var (graph, _) = project.GetGraph();
            var state = Fdg.Create(graph.Positions, graph.Weights, graph.Sizes);
            logger.LogInformation("Processing regular iterations.");
            for (int i = 0; i < RegularIterationCount; ++i)
            {
                Fdg.Step(ref state);
            }
            logger.LogInformation("Processing overlap prevention iterations.");
            state.PreventOverlapping = true;
            for (int i = 0; i < NoOverlapIterationCount; ++i)
            {
                Fdg.Step(ref state);
            }
            logger.LogInformation("Processing strong gravity iterations.");
            state.IsGravityStrong = true;
            for (int i = 0; i < StrongGravityIterationCount; ++i)
            {
                Fdg.Step(ref state);
            }
            return graph;
        }

        public static async Task RunPipeline(FileSystemInfo source, Dictionary<string, string> properties)
        {
            var nullableSolution = await RunAnalysis(source, properties);
            if (nullableSolution is null)
            {
                return;
            }
            var solution = nullableSolution.Value;

            var solutionGraph = solution.GetGraph();
            var solutionFdgLogger = Logging.CreateLogger($"{solution.Name}: Fdg");
            var fdgCache = IsForced ? null : await Serialize.GetCache<SerializableGraphCollection>(
                FdgCacheFilename,
                solutionFdgLogger);
            var fdgResults = await Task.WhenAll(solutionGraph.Labels
                .Select(p => Task.Run(() => RunFdg(solution.Projects[p], fdgCache))));
            var serializableGraphs = new SerializableGraphCollection
            {
                Graphs = solutionGraph.Labels.Zip(fdgResults)
                    .ToDictionary(p => p.First, p => SerializableGraph
                        .FromGraph(p.Second, solution.Projects[p.First].LastWriteTime))
            };
            await Serialize.SetCache(FdgCacheFilename, serializableGraphs, solutionFdgLogger);
            var sizes = fdgResults.Select(g => 
            {
                var (max, min) = g.GetBoundingBox();
                return MathF.Max(max.X - min.X, max.Y - min.Y);
            }).ToArray();
            var solutionFdgState = Fdg.Create(solutionGraph.Positions, solutionGraph.Weights, sizes);
            solutionFdgState.PreventOverlapping = true;
            for(int i = 0; i < 1000; ++i)
            {
                Fdg.Step(ref solutionFdgState);
            }

            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            await Task.WhenAll(Enumerable.Range(0, solutionGraph.Labels.Length).Select(i => Task.Run(() =>
            {
                var name = solutionGraph.Labels[i];
                var generatorLogger = Logging.CreateLogger($"{name}: Generator");
                var project = solution.Projects[name];
                var graph = fdgResults[i];
                var ids = graph.Labels.Select(l => AnalyzedTypeId.Parse(l)).ToArray();
                var offset = ((int)solutionGraph.Positions[i].X, (int)solutionGraph.Positions[i].Y);
                Terrain.GenerateIsland(world, project, graph.Positions, graph.Sizes, ids, offset, generatorLogger);
            })));

            var builtWorld = world.Build();
            Vku.HelloWorld(builtWorld);
        }

        public static unsafe int Main(string[] args)
        {
            var rootCmd = new RootCommand("A software visualization tool")
            {
                Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(RunPipeline))!)
            };
            rootCmd.AddGlobalOption(new Option<bool>(VkDebugAlias, "Enable Vulkan validation layers"));
            rootCmd.AddGlobalOption(new Option<bool>(new[] { "-v", VerboseAlias }, "Set logging level to Debug"));
            rootCmd.AddGlobalOption(new Option<bool>(new[] { "-f", ForceAlias }, "Overwrite cached results"));
            rootCmd.AddArgument(new Argument<FileSystemInfo>(
                name: "SOURCE",
                description: "Path to a project or a solution",
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory)));
            var propertyOption = new Option<Dictionary<string, string>>(
                aliases: new[] { "-p", "--properties" },
                parseArgument: a => a.Tokens
                    .Select(t => t.Value.Split('=', 2))
                    .ToDictionary(p => p[0], p => p[1]),
                description: "Set an MSBuild property for the loading of projects");
            propertyOption.Argument.AddValidator(r =>
            {
                foreach (var token in r.Tokens)
                {
                    if (!token.Value.Contains('='))
                    {
                        return "MSBuild properties must follow the '<n>=<v>' format.";
                    }
                }
                return null;
            });
            rootCmd.AddOption(propertyOption);

            var debugCmd = new Command("debug", "Runs a debug utility");
            DebugDraw.AddDrawCommands(debugCmd);
            DebugGraph.AddGraphCommands(debugCmd);
            rootCmd.AddCommand(debugCmd);


            var builder = new CommandLineBuilder(rootCmd);
            builder.UseDefaults();
            builder.UseMiddleware(c =>
            {
                IsForced = c.ParseResult.ValueForOption<bool>(ForceAlias);

                if (c.ParseResult.ValueForOption<bool>(VkDebugAlias))
                {
                    Vku.SetDebug(true);
                }

                LogLevel minimumLevel = c.ParseResult.ValueForOption<bool>(VerboseAlias)
                    ? LogLevel.Debug
                    : LogLevel.Information;
                Logging = LoggerFactory.Create(b =>
                {
                    b.AddConsole();
                    b.SetMinimumLevel(minimumLevel);
                });
                var renderLogger = Logging.CreateLogger("Renderer");
                Vku.SetLogCallback((l, m) => renderLogger.Log((LogLevel)l, m));
            });
            builder.Build(); // Sets ImplicitParser inside the root command. Yes, it's weird, I know.
            var errorCode = rootCmd.Invoke(args);
            Logging.Dispose();
            return errorCode;
        }
    }
}
