using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Helveg.Serialization;
using Helveg.Landscape;
using Helveg.Analysis;
using Helveg.Render;
using Microsoft.Build.Locator;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;
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
                logger.LogDebug($"Caching analysis results to '{AnalysisCacheFilename}'.");
                using var stream = new FileStream(AnalysisCacheFilename, FileMode.Create);
                await JsonSerializer.SerializeAsync(
                    stream,
                    SerializableSolution.FromAnalyzed(analyzedSolution.Value),
                    Serialize.JsonOptions);
            }
            return analyzedSolution;
        }

        public static async Task<(Vector2[] positions, float[] sizes, AnalyzedTypeId[] ids)>
            RunFdg(AnalyzedProject project)
        {
            var logger = Logging.CreateLogger("Fdg");
            var serializableGraph = await Serialize.GetCache<SerializableGraph>(FdgCacheFilename, logger);
            if (serializableGraph is object && serializableGraph.Name == project.Name
                && serializableGraph.TimeStamp == project.LastWriteTime
                && serializableGraph.Positions is object
                && serializableGraph.Sizes is object
                && serializableGraph.Ids is object
                && !IsForced)
            {
                logger.LogInformation("Using cached positional results.");
                return (
                    positions: serializableGraph.Positions,
                    sizes: serializableGraph.Sizes,
                    ids: serializableGraph.Ids.Select(id => AnalyzedTypeId.Parse(id)).ToArray());
            }

            var (graph, ids) = project.GetGraph();
            var state = Fdg.Create(graph.Positions, graph.Weights, graph.Sizes);
            logger.LogInformation("Processing regular iterations.");
            for (int i = 0; i < RegularIterationCount; ++i)
            {
                Fdg.Step(state);
            }
            logger.LogInformation("Processing overlap prevention iterations.");
            state.PreventOverlapping = true;
            for (int i = 0; i < NoOverlapIterationCount; ++i)
            {
                Fdg.Step(state);
            }
            logger.LogInformation("Processing strong gravity iterations.");
            state.IsGravityStrong = true;
            for (int i = 0; i < StrongGravityIterationCount; ++i)
            {
                Fdg.Step(state);
            }

            using (var stream = new FileStream(FdgCacheFilename, FileMode.Create))
            {
                serializableGraph = new SerializableGraph
                {
                    TimeStamp = project.LastWriteTime,
                    Name = project.Name,
                    Positions = graph.Positions,
                    Sizes = graph.Sizes,
                    Ids = graph.Labels,
                };
                await JsonSerializer.SerializeAsync(stream, serializableGraph, Serialize.JsonOptions);
            }
            return (graph.Positions, graph.Sizes, ids);
        }

        public static async Task RunPipeline(FileSystemInfo source, Dictionary<string, string> properties)
        {
            var analyzedSolution = await RunAnalysis(source, properties);
            if (analyzedSolution is null)
            {
                return;
            }

            // TODO: Handle multiple projects.
            var analyzedProject = analyzedSolution.Value.Projects.First().Value;
            var (positions, sizes, ids) = await RunFdg(analyzedProject);

            var generatorLogger = Logging.CreateLogger("Generator");
            var world = Terrain.GenerateIsland(analyzedProject, positions, sizes, ids, generatorLogger).Build();
            // foreach (var chunk in world.Chunks)
            // {
            //     chunk.HollowOut(new Block { Flags = BlockFlags.IsAir });
            // }
            Vku.HelloWorld(world);
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
            return rootCmd.Invoke(args);
        }
    }
}
