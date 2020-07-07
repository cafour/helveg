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

namespace Helveg
{
    public class Program
    {
        public const string AnalysisCacheFilename = "helveg-analysis.json";
        public const string FdgCacheFilename = "helveg-fdg.json";
        public const int RegularIterationCount = 5000;
        public const int StrongGravityIterationCount = 500;
        public const int NoOverlapIterationCount = 5000;
        public const string VkDebugAlias = "--vk-debug";
        public const string VerboseAlias = "--verbose";
        public const string ForceAlias = "--force";

        public static ILoggerFactory Logging = new NullLoggerFactory();
        public static bool IsForced = false;

        public static async Task<T?> GetCache<T>(string path, ILogger logger)
            where T : class
        {
            if (File.Exists(path) && !IsForced)
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Open);
                    return await JsonSerializer.DeserializeAsync<T>(stream, Serialize.JsonOptions);
                }
                catch(JsonException e)
                {
                    logger.LogDebug(e, $"Failed to read the '{path}' cache.");
                }
            }
            return null;
        }

        public static async Task<AnalyzedProject?> RunAnalysis(FileSystemInfo source)
        {
            var logger = Logging.CreateLogger("Analysis");
            FileInfo? csprojFile = source switch
            {
                FileInfo f => f.Extension == ".csproj" ? f : null,
                DirectoryInfo d => d.EnumerateFiles("*.csproj").FirstOrDefault(),
                _ => throw new NotImplementedException()
            };

            if (csprojFile is null)
            {
                logger.LogCritical($"No '.csproj' file could be located in the '{source}' directory.");
                return null;
            }

            logger.LogDebug($"Found '{csprojFile.FullName}'.");

            var serializableProject = await GetCache<SerializableProject>(AnalysisCacheFilename, logger);
            if (serializableProject is object && serializableProject.CsprojPath == csprojFile.FullName)
            {
                logger.LogInformation("Using cached analysis results.");
                return serializableProject.ToAnalyzed();
            }

            var vsInstance = MSBuildLocator.RegisterDefaults();
            logger.LogDebug($"Using MSBuild at '{vsInstance.MSBuildPath}'.");

            logger.LogInformation("Analyzing project.");
            var analyzedProject = await Analyze.AnalyzeProject(csprojFile, logger);
            if (analyzedProject is object)
            {
                using var stream = new FileStream(AnalysisCacheFilename, FileMode.Create);
                await JsonSerializer.SerializeAsync(
                    stream,
                    SerializableProject.FromAnalyzed(csprojFile.FullName, analyzedProject.Value),
                    Serialize.JsonOptions);
            }
            return analyzedProject;
        }

        public static async Task<ImmutableDictionary<AnalyzedTypeId, Vector2>> RunFdg(AnalyzedProject project)
        {
            var logger = Logging.CreateLogger("Fdg");
            var serializableGraph = await GetCache<SerializableGraph>(FdgCacheFilename, logger);
            if (serializableGraph is object && serializableGraph.Name == project.Name)
            {
                logger.LogInformation("Using cached positional results.");
                return serializableGraph.Positions.ToImmutableDictionary(
                    p => AnalyzedTypeId.Parse(p.Key),
                    p => p.Value);
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

        public static async Task RunPipeline(FileSystemInfo source)
        {
            var analyzedProject = await RunAnalysis(source);
            if (analyzedProject is null)
            {
                return;
            }

            var positions = await RunFdg(analyzedProject.Value);

            var world = Terrain.GenerateIsland(analyzedProject.Value, positions).Build();
            foreach (var chunk in world.Chunks)
            {
                chunk.HollowOut(new Block { Flags = BlockFlags.IsAir });
            }
            Vku.HelloWorld(world);
        }

        public static unsafe int Main(string[] args)
        {
            var rootCmd = new RootCommand("A software visualization tool")
            {
                Handler = CommandHandler.Create<FileSystemInfo>(RunPipeline)
            };
            rootCmd.AddGlobalOption(new Option<bool>(VkDebugAlias, "Enable Vulkan validation layers"));
            rootCmd.AddGlobalOption(new Option<bool>(new[] { "-v", VerboseAlias }, "Set logging level to Debug"));
            rootCmd.AddGlobalOption(new Option<bool>(new[] { "-f", ForceAlias}, "Overwrite cached results"));
            rootCmd.AddArgument(new Argument<FileSystemInfo>(
                name: "SOURCE",
                description: "Path to a project or a solution",
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory)));

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
