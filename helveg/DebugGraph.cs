using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Helveg.Analysis;
using Helveg.Landscape;
using Helveg.Render;
using Helveg.Serialization;
using Microsoft.Extensions.Logging;

namespace Helveg
{
    public static class DebugGraph
    {
        public enum GraphFormat
        {
            Vku,
            GraphViz
        }

        public static void AddGraphCommands(Command parent)
        {
            var formatOpt = new Option<GraphFormat>(
                aliases: new[] { "-f", "--format" },
                getDefaultValue: () => GraphFormat.Vku,
                description: "The format of the animation");
            var iterationsOpt = new Option<int>(
                aliases: new[] { "-i", "--iterations" },
                getDefaultValue: () => 1000,
                description: "The number of iterations");
            var speedOpt = new Option<int>(
                aliases: new[] { "-s", "--speed" },
                getDefaultValue: () => 100,
                description: "The length of a single frame in ms (Vku)");
            var everyOpt = new Option<int>(
                aliases: new[] { "-e", "--every" },
                getDefaultValue: () => 100,
                description: "Save node positions every n iterations (GraphViz)");
            var ignoreCacheOpt = new Option<FileInfo>(
                aliases: new[] { "--json" },
                description: "Path to a JSON document with results of an analysis");

            var eadesCmd = new Command("eades", "Animates the Eades'84 algorithm")
            {
                formatOpt,
                iterationsOpt,
                speedOpt,
                everyOpt,
                ignoreCacheOpt
            };
            eadesCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunEades))!);
            parent.AddCommand(eadesCmd);

            var fdgCmd = new Command("fdg", "Animates the ForceAtlas2-inspired algorithm")
            {
                formatOpt,
                new Option<int>(
                    aliases: new [] {"-r", "--regular"},
                    getDefaultValue: () => 1000,
                    description: "The number of regular iterations"),
                new Option<int>(
                    aliases: new [] {"-g", "--strong-gravity"},
                    getDefaultValue: () => 500,
                    description: "The number of strong gravity iterations"),
                new Option<int>(
                    aliases: new [] {"-o", "--no-overlap"},
                    getDefaultValue: () => 500,
                    description: "The number of overlap preventing iterations"),
                speedOpt,
                everyOpt,
                ignoreCacheOpt
            };
            fdgCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunFdg))!);
            parent.AddCommand(fdgCmd);

            var islandGraphCmd = new Command("island-graph", "Animates the layout of islands");
            islandGraphCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunIslandGraph))!);
            parent.AddCommand(islandGraphCmd);
        }

        public static async Task RunIslandGraph(FileSystemInfo source, Dictionary<string, string> properties)
        {
            var logger = Program.Logging.CreateLogger($"Debug Island Graph");
            var nullableSolution = await Program.RunAnalysis(source, properties);
            if (nullableSolution is null)
            {
                logger.LogCritical("Failed to load the solution.");
                return;
            }

            var solution = nullableSolution.Value;
            var solutionGraph = Graph.FromAnalyzed(solution);
            var graphs = await Task.WhenAll(solutionGraph.Labels
                .Select(p => Task.Run(() =>
                {
                    var project = solution.Projects[p];
                    var graph = Graph.FromAnalyzed(project);
                    graph.LayInCircle(graph.Positions.Length);
                    for (int i = 0; i < graph.Positions.Length; ++i)
                    {
                        graph.Sizes[i] = MathF.Max(graph.Sizes[i], Program.MinNodeSize);
                    }
                    return Program.RunFdg(
                        graph: graph,
                        regularCount: Program.RegularIterationCount,
                        strongGravityCount: Program.StrongGravityIterationCount,
                        overlapPreventionCount: Program.NoOverlapIterationCount,
                        expectedTimeStamp: project.LastWriteTime);
                })));

            for (int i = 0; i < graphs.Length; ++i)
            {
                var bbox = graphs[i].GetBoundingBox();
                solutionGraph.Sizes[i] = MathF.Max(bbox.Width, bbox.Height);
            }
            logger.LogInformation("Laying out islands.");
            solutionGraph.LayInCircle(solutionGraph.Positions.Length);
            // var state = Eades.Create(solutionGraph.Positions, solutionGraph.Weights);
            var maxSize = solutionGraph.Sizes.Max();
            // state.UnloadedLength = maxSize + Program.IslandGapSize;
            // state.Gravity = 0.5f;
            // // state.UnloadedLength = maxSize + Program.IslandGapSize;
            // // state.Repulsion *= state.UnloadedLength;
            // state.Stiffness = 20;
            // state.Repulsion = 10;
            // DrawGraph(
            //     graph: solutionGraph,
            //     step: (i, g) =>
            //     {
            //         Eades.Step(ref state);
            //         return g;
            //     },
            //     format: GraphFormat.Vku,
            //     iterationCount: Program.IslandIterationCount,
            //     prefix: string.Empty,
            //     every: -1,
            //     speed: 10);

            float scale = 1.0f / (solutionGraph.Positions.Length * maxSize);
            var state = Fdg.Create(solutionGraph.Positions, solutionGraph.Weights, solutionGraph.Sizes);
            state.RepulsionFactor = state.Positions.Length * maxSize;
            state.GravityFactor = 0.5f;
            state.IsGravityStrong = true;
            solutionGraph = DrawGraph(
                graph: solutionGraph,
                step: (i, g) =>
                {
                    Fdg.Step(ref state);
                    return g;
                },
                format: GraphFormat.Vku,
                iterationCount: 3000,
                prefix: "",
                every: -1,
                speed: 10,
                scale: scale);
            state.PreventOverlapping = true;
            solutionGraph = DrawGraph(
                graph: solutionGraph,
                step: (i, g) =>
                {
                    Fdg.Step(ref state);
                    return g;
                },
                format: GraphFormat.Vku,
                iterationCount: 2000,
                prefix: "",
                every: -1,
                speed: 10,
                scale: scale);
            // state.IsGravityStrong = true;
            // solutionGraph = DrawGraph(
            //     graph: solutionGraph,
            //     step: (i, g) =>
            //     {
            //         Fdg.Step(ref state);
            //         return g;
            //     },
            //     format: GraphFormat.Vku,
            //     iterationCount: 1000,
            //     prefix: "",
            //     every: -1,
            //     speed: 10,
            //     scale: scale);
        }

        public static async Task RunEades(
            FileSystemInfo source,
            FileInfo json,
            Dictionary<string, string> properties,
            GraphFormat format,
            int iterations,
            int speed,
            int every)
        {
            Eades.State state = default;
            await DrawGraph(
                source: source,
                json: json,
                properties: properties,
                init: g =>
                {
                    state = Eades.Create(g.Positions, g.Weights);
                    state.UnloadedLength = g.Positions.Length / 2f;
                    state.Repulsion = g.Positions.Length;
                },
                step: (i, g) =>
                {
                    Eades.Step(ref state);
                    return g;
                },
                format: format,
                iterationCount: iterations,
                prefix: "eades",
                every: every,
                speed: speed);
        }

        public static async Task RunFdg(
            FileSystemInfo source,
            FileInfo json,
            Dictionary<string, string> properties,
            GraphFormat format,
            int regular,
            int strongGravity,
            int noOverlap,
            int speed,
            int every)
        {
            var logger = Program.Logging.CreateLogger("Debug Fdg");
            Fdg.State state = default;
            var graph = await SetUpGraph(
                source: source,
                json: json,
                properties: properties,
                init: g =>
                {
                    state = Fdg.Create(g.Positions, g.Weights, g.Sizes);
                });
            if (graph is null)
            {
                return;
            }

            if (regular > 0)
            {
                logger.LogInformation("Processing regular iterations.");
                graph = DrawGraph(
                    graph: graph.Value,
                    step: (i, g) =>
                    {
                        Fdg.Step(ref state);
                        return g;
                    },
                    format: format,
                    iterationCount: regular,
                    prefix: "fdg-regular",
                    every: every,
                    speed: speed);
            }
            if (noOverlap > 0)
            {
                logger.LogInformation("Processing overlap prevention iterations.");
                state.PreventOverlapping = true;
                DrawGraph(
                    graph: graph.Value,
                    step: (i, g) =>
                    {
                        Fdg.Step(ref state);
                        return g;
                    },
                    format: format,
                    iterationCount: noOverlap,
                    prefix: "fdg-no-overlap",
                    every: every,
                    speed: speed);
            }
            if (strongGravity > 0)
            {
                logger.LogInformation("Processing strong gravity iterations.");
                state.IsGravityStrong = true;
                graph = DrawGraph(
                    graph: graph.Value,
                    step: (i, g) =>
                    {
                        Fdg.Step(ref state);
                        return g;
                    },
                    format: format,
                    iterationCount: strongGravity,
                    prefix: "fdg-strong-gravity",
                    every: every,
                    speed: speed);
            }
        }

        private static async Task<Graph?> DrawGraph(
            FileSystemInfo source,
            FileInfo json,
            IDictionary<string, string> properties,
            Action<Graph> init,
            Func<int, Graph, Graph> step,
            GraphFormat format,
            int iterationCount,
            string prefix,
            int every,
            int speed)
        {
            var graph = await SetUpGraph(source, json, properties, init);
            if (graph is null)
            {
                return null;
            }

            DrawGraph(graph.Value, step, format, iterationCount, prefix, every, speed);
            return graph;
        }

        private static async Task<Graph?> SetUpGraph(
            FileSystemInfo source,
            FileInfo? json,
            IDictionary<string, string> properties,
            Action<Graph> init)
        {
            AnalyzedProject? project = null;
            if (json is object)
            {
                using var stream = new FileStream(json.FullName, FileMode.Open);
                project = (await JsonSerializer.DeserializeAsync<SerializableSolution>(stream, Serialize.JsonOptions))
                    .ToAnalyzed()
                    .Projects
                    .First()
                    .Value;
            }
            else
            {
                var solution = await Program.RunAnalysis(source, properties);
                if (solution is object)
                {
                    project = solution.Value.Projects.First().Value;
                }
            }
            if (project is null)
            {
                return null;
            }

            var graph = Graph.FromAnalyzed(project.Value);
            init(graph);
            graph.LayInCircle(graph.Positions.Length);
            return graph;
        }

        private static Graph DrawGraph(
            Graph graph,
            Func<int, Graph, Graph> step,
            GraphFormat format,
            int iterationCount,
            string prefix,
            int every,
            int speed,
            float scale = 0.01f)
        {
            switch (format)
            {
                case GraphFormat.Vku:
                    DrawVku(graph, step, iterationCount, speed, scale);
                    break;
                case GraphFormat.GraphViz:
                    DrawGraphViz(graph, step, iterationCount, prefix, every);
                    break;
                default:
                    throw new NotSupportedException($"Graph format '{format}' is not supported.");
            }
            return graph;
        }

        private static void DrawGraphViz(
            Graph graph,
            Func<int, Graph, Graph> step,
            int iterationCount,
            string prefix,
            int every)
        {
            int fieldWidth = (int)Math.Log10(iterationCount) + 1;
            File.WriteAllText($"{prefix}_{0.ToString().PadLeft(fieldWidth, '0')}.gv", graph.ToGraphviz());
            for (int i = 0; i < iterationCount; ++i)
            {
                graph = step(i, graph);
                if (i % every == 0)
                {
                    File.WriteAllText($"{prefix}_{i.ToString().PadLeft(fieldWidth, '0')}.gv", graph.ToGraphviz());
                }
            }
            File.WriteAllText($"{prefix}_{iterationCount.ToString().PadLeft(fieldWidth, '0')}.gv", graph.ToGraphviz());
        }

        private static void DrawVku(
            Graph graph,
            Func<int, Graph, Graph> step,
            int iterationCount,
            int speed,
            float scale)
        {
            var render = Vku.CreateGraphRender(graph, scale);
            Vku.StepGraphRender(render);
            var stopwatch = new Stopwatch();
            var end = false;
            for (int i = 0; i < iterationCount && !end; ++i)
            {
                graph = step(i, graph);
                end = Vku.StepGraphRender(render);
                Thread.Sleep((int)(speed - stopwatch.ElapsedMilliseconds % speed));
            }
            Vku.DestroyGraphRender(render);
        }
    }
}
