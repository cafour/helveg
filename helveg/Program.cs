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
using System.Drawing;
using System.Numerics;

namespace Helveg
{
    public class Program
    {
        public const string AnalysisCacheFilename = "helveg-analysis.json";
        public const string FdgCacheFilename = "helveg-fdg.json";
        public const int RegularIterationCount = 2000;
        public const int NoOverlapIterationCount = 4000;
        public const int StrongGravityIterationCount = 500;
        public const int IslandIterationCount = 1000;
        public const float MinNodeSize = 6;
        public const float IslandGapSize = 500f;
        public const string VkDebugAlias = "--vk-debug";
        public const string VerboseAlias = "--verbose";
        public const string ForceAlias = "--force";
        public const string RayTracingAlias = "--ray-tracing";
        public const string ForceCursorAlias = "--force-cursor";
        public const int DefaultCameraElevation = 12;

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
            NuGetLocator.Register(vsInstance.MSBuildPath);
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

        public static Graph RunFdg(
            Graph graph,
            int regularCount,
            int strongGravityCount,
            int overlapPreventionCount,
            DateTime expectedTimeStamp = default,
            SerializableGraphCollection? cache = null,
            float repulsionFactor = 2.0f)
        {
            var logger = Logging.CreateLogger(string.IsNullOrEmpty(graph.Name) ? "Fdg" : $"{graph.Name}: Fdg");
            if (cache is object
                && cache.Graphs is object
                && cache.Graphs.TryGetValue(graph.Name, out var cachedGraph)
                && cachedGraph.TimeStamp == expectedTimeStamp)
            {
                logger.LogInformation("Using cached Fdg results.");
                return cachedGraph.ToGraph();
            }

            var state = Fdg.Create(graph.Positions, graph.Weights, graph.Sizes);
            state.RepulsionFactor = repulsionFactor;
            logger.LogInformation("Processing regular iterations.");
            for (int i = 0; i < regularCount; ++i)
            {
                Fdg.Step(ref state);
            }
            logger.LogInformation("Processing strong gravity iterations.");
            state.IsGravityStrong = true;
            for (int i = 0; i < strongGravityCount; ++i)
            {
                Fdg.Step(ref state);
            }
            logger.LogInformation("Processing overlap prevention iterations.");
            state.IsGravityStrong = false;
            state.PreventOverlapping = true;
            for (int i = 0; i < overlapPreventionCount; ++i)
            {
                Fdg.Step(ref state);
            }
            return graph;
        }

        public static async Task<World> RunLandscapeGeneration(AnalyzedSolution solution)
        {
            var logger = Logging.CreateLogger($"Generator");
            var cache = IsForced
                ? null
                : await Serialize.GetCache<SerializableGraphCollection>(FdgCacheFilename, logger);

            var solutionGraph = Graph.FromAnalyzed(solution);
            var graphs = await Task.WhenAll(solutionGraph.Labels
                .Select(p => Task.Run(() =>
                {
                    var project = solution.Projects[p];
                    var graph = Graph.FromAnalyzed(project);
                    graph.LayInCircle(graph.Positions.Length);
                    for (int i = 0; i < graph.Positions.Length; ++i)
                    {
                        graph.Sizes[i] = MathF.Max(graph.Sizes[i], MinNodeSize);
                    }
                    return RunFdg(
                        graph: graph,
                        regularCount: RegularIterationCount,
                        strongGravityCount: StrongGravityIterationCount,
                        overlapPreventionCount: NoOverlapIterationCount,
                        expectedTimeStamp: project.LastWriteTime,
                        cache: cache);
                })));

            var serializableGraphs = new SerializableGraphCollection
            {
                Graphs = solutionGraph.Labels.Zip(graphs)
                    .ToDictionary(p => p.First, p => SerializableGraph
                        .FromGraph(p.Second, solution.Projects[p.First].LastWriteTime))
            };
            await Serialize.SetCache(FdgCacheFilename, serializableGraphs, logger);

            for (int i = 0; i < graphs.Length; ++i)
            {
                var bbox = graphs[i].GetBoundingBox();
                solutionGraph.Sizes[i] = MathF.Max(bbox.Width, bbox.Height);
            }
            logger.LogInformation("Laying out islands.");
            solutionGraph.LayInCircle();
            // var state = Eades.Create(solutionGraph.Positions, solutionGraph.Weights);
            var maxSize = solutionGraph.Sizes.Max();
            var solutionFdgState = Fdg.Create(solutionGraph.Positions, solutionGraph.Weights, solutionGraph.Sizes);
            solutionFdgState.RepulsionFactor = solutionGraph.Positions.Length * maxSize;
            solutionFdgState.IsGravityStrong = true;
            solutionFdgState.GravityFactor = 0.5f;
            for (int i = 0; i < 3000; ++i)
            {
                Fdg.Step(ref solutionFdgState);
            }
            solutionFdgState.PreventOverlapping = true;
            for (int i = 0; i < 2000; ++i)
            {
                Fdg.Step(ref solutionFdgState);
            }
            // state.UnloadedLength = maxSize + IslandGapSize;
            // state.Repulsion = maxSize;
            // for (int i = 0; i < IslandIterationCount; ++i)
            // {
            //     Eades.Step(ref state);
            // }

            const int margin = 64;
            var globalBbox = Rectangle.Round(RectangleF.Inflate(solutionGraph.GetBoundingBox(), margin, margin));
            var heightmap = new Heightmap(globalBbox);
            logger.LogInformation("Writing ocean heightmap.");
            Terrain.WriteOceanFloorHeightmap(heightmap, solution.GetSeed());
            for (int i = 0; i < graphs.Length; ++i)
            {
                var graph = graphs[i];
                var pos = solutionGraph.Positions[i];
                for (int j = 0; j < graph.Positions.Length; ++j)
                {
                    graph.Positions[j] += pos;
                }
                var radius = solutionGraph.Sizes[i];
                var area = Rectangle.Round(RectangleF.Inflate(graph.GetBoundingBox(), margin, margin));
                var project = solution.Projects[solutionGraph.Labels[i]];
                logger.LogInformation($"Writing island heightmap of '{project.Name}'.");
                Terrain.WriteIslandHeightmap(heightmap, area, project.GetSeed(), graph.Positions, graph.Sizes);
            }

            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, Colors.IslandPalette);
            logger.LogInformation("Generating terrain.");
            Terrain.GenerateTerrain(heightmap, world);

            for (int i = 0; i < graphs.Length; ++i)
            {
                var project = solution.Projects[solutionGraph.Labels[i]];
                logger.LogInformation($"Placing structures of '{project.Name}'.");
                var bbox = graphs[i].GetBoundingBox();
                if (!Terrain.PlaceBridges(heightmap, world, solutionGraph, project))
                {
                    logger.LogError($"Bridge from the '{project.Name}' island could not be placed.");
                }
                Terrain.PlaceCargo(world, project, bbox);
                Terrain.PlaceTypeStructures(heightmap, world, project, graphs[i].ToAnalyzed());
            }
            var cameraHeight = heightmap[heightmap.CenterX, heightmap.CenterY];
            world.InitialCameraPosition = new Vector3(
                heightmap.CenterX,
                cameraHeight + DefaultCameraElevation,
                heightmap.CenterY);

            return world.Build();
        }

        public static async Task<int> RunPipeline(FileSystemInfo source, Dictionary<string, string> properties)
        {
            // code analysis
            var solution = await RunAnalysis(source, properties);
            if (solution is null)
            {
                return 1;
            }

            // landscape generation
            var world = await RunLandscapeGeneration(solution.Value);

            // rendering
            Vku.HelloWorld(world);
            return 0;
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
            rootCmd.AddGlobalOption(new Option<bool>(ForceCursorAlias, "Never hide cursor"));
            rootCmd.AddGlobalOption(new Option<bool>(
                alias: RayTracingAlias,
                description: "Enable rendering with VK_KHR_ray_tracing"));
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

                if (c.ParseResult.ValueForOption<bool>(RayTracingAlias))
                {
                    Vku.SetRayTracing(true);
                }

                if (c.ParseResult.ValueForOption<bool>(ForceCursorAlias))
                {
                    Vku.SetForceCursor(true);
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
