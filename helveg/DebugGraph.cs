using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Helveg.Landscape;
using Helveg.Render;

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
            var eadesCmd = new Command("eades", "Animates the Eades'84 algorithm")
            {
                new Argument<FileInfo>("csproj", "Path to an MSBuild C# project"),
                new Option<GraphFormat>(new []{"-f", "--format"}, "The format of the animation"),
                new Option<int>(new [] {"-i", "--iterations"}, "The number of iterations" ),
                new Option<int>(new [] {"-s", "--speed"}, "The length of a single frame in ms (Vku)"),
                new Option<int>(new [] {"-e", "--every"}, "Save node positions after every n iterations (GraphViz)"),
                new Option<bool>("--ignore-cache", "Ignore cached analysis results")
            };
            eadesCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunEades))!);
            parent.AddCommand(eadesCmd);

            var frCmd = new Command("fr", "Animates the Fruchterman-Reingold algorithm")
            {
                new Argument<FileInfo>("csproj", "Path to an MSBuild C# project"),
                new Option<GraphFormat>(new []{"-f", "--format"}, "The format of the animation"),
                new Option<int>(new [] {"-i", "--iterations"}, "The number of iterations" ),
                new Option<float>(new [] {"-w", "--width"}, "The width of the graph space" ),
                new Option<float>(new [] {"-h", "--height"}, "The height of the graph space" ),
                new Option<int>(new [] {"-s", "--speed"}, "The length of a single frame in ms (Vku)"),
                new Option<int>(new [] {"-e", "--every"}, "Save node positions after every n iterations (GraphViz)"),
                new Option<bool>("--ignore-cache", "Ignore cached analysis results")
            };
            frCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunFruchtermanReingold))!);
            parent.AddCommand(frCmd);

            var fdgCmd = new Command("fdg", "Animates the ForceAtlas2-inspired algorithm")
            {
                new Argument<FileInfo>("csproj", "Path to an MSBuild C# project"),
                new Option<GraphFormat>(new []{"-f", "--format"}, "The format of the animation"),
                new Option<int>(new [] {"-r", "--regular"}, "The number of regular iterations" ),
                new Option<int>(new [] {"-g", "--strong-gravity"}, "The number of strong gravity iterations" ),
                new Option<int>(new [] {"-o", "--no-overlap"}, "The number of overlap preventing iterations" ),
                new Option<int>(new [] {"-s", "--speed"}, "The length of a single frame in ms (Vku)"),
                new Option<int>(new [] {"-e", "--every"}, "Save node positions after every n iterations (GraphViz)"),
                new Option<bool>("--ignore-cache", "Ignore cached analysis results")
            };
            fdgCmd.Handler = CommandHandler.Create(typeof(DebugGraph).GetMethod(nameof(RunFdg))!);
            parent.AddCommand(fdgCmd);
        }

        public static async Task RunEades(
            FileInfo csproj,
            GraphFormat format,
            int iterations,
            int speed,
            int every,
            bool ignoreCache = false)
        {
            Eades.State state = default;
            await DrawGraph(
                csproj: csproj,
                init: g =>
                {
                    state = Eades.Create(g.Positions.Length, g.Weights);
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
                speed: speed,
                ignoreCache: ignoreCache);
        }

        public static async Task RunFruchtermanReingold(
            FileInfo csproj,
            GraphFormat format = GraphFormat.Vku,
            int iterations = 1000,
            float width = 1024.0f,
            float height = 1024.0f,
            int speed = 100,
            int every = 100,
            bool ignoreCache = false)
        {
            FR.State state = default;
            await DrawGraph(
                csproj: csproj,
                init: g =>
                {
                    state = FR.Create(g.Positions.Length, g.Weights, width, height);
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
                speed: speed,
                ignoreCache: ignoreCache);
        }

        public static async Task RunFdg(
            FileInfo csproj,
            GraphFormat format = GraphFormat.Vku,
            int regular = 1000,
            int strongGravity = 500,
            int noOverlap = 500,
            int speed = 100,
            int every = 100,
            bool ignoreCache = false)
        {
            Fdg.State state = default;
            var graph = await DrawGraph(
                csproj: csproj,
                init: g =>
                {
                    state = Fdg.Create(g.Positions.Length, g.Weights);
                },
                step: (i, g) =>
                {
                    Fdg.Step(ref state);
                    return g;
                },
                format: format,
                iterationCount: regular,
                prefix: "fdg-regular",
                every: every,
                speed: speed,
                ignoreCache: ignoreCache);
            state.IsGravityStrong = true;
            graph = DrawGraph(
                graph: graph,
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
            state.IsGravityStrong = false;
            state.PreventOverlapping = true;
            DrawGraph(
                graph: graph,
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

        private static async Task<Graph> DrawGraph(
            FileInfo csproj,
            Action<Graph> init,
            Func<int, Graph, Graph> step,
            GraphFormat format,
            int iterationCount,
            string prefix,
            int every,
            int speed,
            bool ignoreCache)
        {
            var project = await Program.RunAnalysis(csproj.FullName, ignoreCache);
            var (names, matrix) = project.GetWeightMatrix();
            var weights = Graph.UndirectWeights(matrix);
            var labels = names.Select(n => n.ToString()).ToArray();
            var graph = new Graph(new Vector2[matrix.GetLength(0)], weights, labels);
            init(graph);
            for (int i = 0; i < graph.Positions.Length; ++i)
            {
                var angle = 2 * MathF.PI / graph.Positions.Length * i;
                graph.Positions[i] = 64f * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }

            DrawGraph(graph, step, format, iterationCount, prefix, every, speed);
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
            switch(format)
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
