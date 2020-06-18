using System.Collections.Generic;
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

namespace Helveg
{
    public class Program
    {
        public const string AnalysisCacheFilename = "helveg-analysis.json";
        public const string FdgCacheFilename = "helveg-fdg.json";
        public const int RegularIterationCount = 5000;
        public const int StrongGravityIterationCount = 500;
        public const int NoOverlapIterationCount = 5000;

        public static ILoggerFactory Logging = new NullLoggerFactory();

        public static async Task<AnalyzedProject> RunAnalysis(string csprojPath, bool ignoreCache = false)
        {
            var logger = Logging.CreateLogger("Analysis");
            if (File.Exists(AnalysisCacheFilename) && !ignoreCache)
            {
                using var stream = new FileStream(AnalysisCacheFilename, FileMode.Open);
                var project = await JsonSerializer.DeserializeAsync<SerializableProject>(stream, Serialize.JsonOptions);
                if (project.CsprojPath == csprojPath)
                {
                    logger.LogInformation("Using cached analysis results.");
                    return project.ToAnalyzed();
                }
            }

            logger.LogInformation("Analyzing project.");
            MSBuildLocator.RegisterDefaults();
            var analyzedProject = await Analyze.AnalyzeProject(csprojPath);
            {
                using var stream = new FileStream(AnalysisCacheFilename, FileMode.Create);
                await JsonSerializer.SerializeAsync(
                    stream,
                    SerializableProject.FromAnalyzed(csprojPath, analyzedProject),
                    Serialize.JsonOptions);
            }
            return analyzedProject;
        }

        public static async Task<ImmutableDictionary<AnalyzedTypeId, Vector2>> RunFdg(
            AnalyzedProject project,
            bool ignoreCache = false)
        {
            var logger = Logging.CreateLogger("Fdg");

            if (File.Exists(FdgCacheFilename) && !ignoreCache)
            {
                using var stream = new FileStream(FdgCacheFilename, FileMode.Open);
                var graph = await JsonSerializer.DeserializeAsync<SerializableGraph>(
                    stream,
                    Serialize.JsonOptions);
                if (graph.Name == project.Name)
                {
                    logger.LogInformation("Using cached positional results.");
                    return graph.Positions.ToImmutableDictionary(
                        p => AnalyzedTypeId.Parse(p.Key),
                        p => p.Value);
                }
            }

            var (names, matrix) = project.GetWeightMatrix();
            var weights = Graph.UndirectWeights(matrix);
            var positions = new Vector2[names.Length];
            var state = Fdg.Create(positions, weights);
            state.NodeSize = 16;
            logger.LogInformation("Processing regular iterations.");
            for (int i = 0; i < RegularIterationCount; ++i)
            {
                Fdg.Step(ref state);
            }
            logger.LogInformation("Processing strong gravity iterations.");
            state.IsGravityStrong = true;
            for (int i = 0; i < StrongGravityIterationCount; ++i)
            {
                Fdg.Step(ref state);
            }
            logger.LogInformation("Processing overlap prevention iterations.");
            state.IsGravityStrong = false;
            state.PreventOverlapping = true;
            for (int i = 0; i < NoOverlapIterationCount; ++i)
            {
                Fdg.Step(ref state);
            }

            var results = names.Zip(positions).ToImmutableDictionary(p => p.First, p => p.Second);
            {
                var stream = new FileStream(FdgCacheFilename, FileMode.Create);
                var graph = new SerializableGraph
                {
                    Name = project.Name,
                    Positions = results.ToDictionary(p => p.Key.ToString(), p => p.Value)
                };
                await JsonSerializer.SerializeAsync(stream, graph, Serialize.JsonOptions);
            }
            return results;
        }

        public static async Task RunPipeline(FileSystemInfo project, bool ignoreCache = false)
        {
            var analyzedProject = await RunAnalysis(project.FullName, ignoreCache);
            var positions = await RunFdg(analyzedProject, ignoreCache);

            var world = Terrain.GenerateIsland(analyzedProject, positions).Build();
            foreach (var chunk in world.Chunks)
            {
                chunk.HollowOut(new Block { Flags = BlockFlags.IsAir });
            }
            Vku.HelloWorld(world);
        }

        public static unsafe int Main(string[] args)
        {
            Logging = LoggerFactory.Create(b =>
            {
                b.AddConsole();
            });

            var rootCmd = new RootCommand("A software visualization tool")
            {
                new Argument<FileSystemInfo>("project", "Path to an MSBuild project"),
                new Option<bool>("--ignore-cache", "Ignored cached results")
            };
            rootCmd.Handler = CommandHandler.Create<FileSystemInfo, bool>(RunPipeline);

            var debugCmd = new Command("debug", "Runs a debug utility");
            DebugDraw.AddDrawCommands(debugCmd);
            DebugGraph.AddGraphCommands(debugCmd);
            rootCmd.AddCommand(debugCmd);

            return rootCmd.Invoke(args);
        }
    }
}
