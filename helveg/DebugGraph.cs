using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
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
                alias: "--json",
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

            var frCmd = new Command("fr", "Animates the Fruchterman-Reingold algorithm")
            {
                formatOpt,
                iterationsOpt,
                new Option<float>(
                    aliases: new [] {"-w", "--width"},
                    getDefaultValue: () => 1024.0f,
                    description: "The width of the graph space" ),
                new Option<float>(
                    aliases: new [] {"-h", "--height"},
                    getDefaultValue: () => 1024.0f,
                    description: "The height of the graph space" ),
                speedOpt,
                everyOpt,
                ignoreCacheOpt
            };
            frCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunFruchtermanReingold))!);
            parent.AddCommand(frCmd);

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

        public static async Task RunFruchtermanReingold(
            FileSystemInfo source,
            FileInfo json,
            Dictionary<string, string> properties,
            GraphFormat format,
            int iterations,
            float width,
            float height,
            int speed,
            int every)
        {
            FR.State state = default;
            await DrawGraph(
                source: source,
                json: json,
                properties: properties,
                init: g =>
                {
                    state = FR.Create(g.Positions, g.Weights, width, height);
                },
                step: (i, g) =>
                {
                    FR.Step(ref state);
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
                        Fdg.Step(state);
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
                        Fdg.Step(state);
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
                        Fdg.Step(state);
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

            var (graph, _) = project.Value.GetGraph();
            init(graph);
            for (int i = 0; i < graph.Positions.Length; ++i)
            {
                var angle = 2 * MathF.PI / graph.Positions.Length * i;
                graph.Positions[i] = 64f * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            return graph;
        }

        private static Graph DrawGraph(
            Graph graph,
            Func<int, Graph, Graph> step,
            GraphFormat format,
            int iterationCount,
            string prefix,
            int every,
            int speed)
        {
            switch (format)
            {
                case GraphFormat.Vku:
                    DrawVku(graph, step, iterationCount, speed);
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
            int speed)
        {
            var render = Vku.CreateGraphRender(graph);
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
