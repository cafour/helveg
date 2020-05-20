using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Build.Locator;

namespace Helveg
{
    public class Program
    {
        public enum ProjectRenderKind
        {
            Vku,
            Graphviz
        };

        public static void WriteSentence(IList<Spruce.Symbol> sentence)
        {
            var prefix = "";
            foreach (var symbol in sentence)
            {
                if (symbol.Kind == Spruce.Kind.Push)
                {
                    Console.WriteLine($"{prefix}[");
                    prefix += "\t";
                    continue;
                }
                else if (symbol.Kind == Spruce.Kind.Pop)
                {
                    prefix = prefix.Substring(0, prefix.Length - 1);
                    Console.WriteLine($"{prefix}]");
                    continue;
                }
                Console.WriteLine($"{prefix}Kind={symbol.Kind},Parameter={symbol.Int}");
            }
            Console.WriteLine($"Sentence length: {sentence.Count}");
        }

        public static void DrawTriangle()
        {
            Vku.HelloTriangle();
        }

        public static int DrawCube()
        {
            var positions = new[]{
                new Vector3(1, 1, 1),
                new Vector3(1, 1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, -1, 1),
                new Vector3(-1, 1, 1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, -1, -1),
                new Vector3(-1, -1, 1)
            };

            var colors = positions.Select(v => (v + new Vector3(1)) / 2)
                .ToArray();

            var indices = new int[] {
                3, 2, 0, 0, 2, 1,
                2, 6, 1, 1, 6, 5,
                6, 7, 5, 5, 7, 4,
                7, 3, 4, 4, 3, 0,
                0, 1, 4, 4, 1, 5,
                2, 3, 6, 6, 3, 7
            };

            var mesh = new Mesh(positions, colors, indices);
            return Vku.HelloMesh(mesh);
        }

        public static int DrawTree()
        {
            var sentence = Spruce.Rewrite(
                axiom: new[] { new Spruce.Symbol(Spruce.Kind.Canopy) },
                seed: 42,
                branchCount: 12,
                maxBranching: 6,
                minBranching: 3,
                initialBranching: 4,
                branchingDiff: 2);
            WriteSentence(sentence);
            var spruceMesh = Spruce.GenerateMesh(sentence);
            Console.WriteLine($"VertexCount: {spruceMesh.Vertices.Length}");

            return Vku.HelloMesh(spruceMesh);
        }

        public static void DrawEades()
        {
            var (names, weights) = RunAnalysis(string.Empty, false);
            var graph = new InteropGraph(new Vector2[weights.GetLength(0)], Array.Empty<float>());
            for (int i = 0; i < graph.Positions.Length; ++i)
            {
                var angle = 2 * MathF.PI / graph.Positions.Length * i;
                graph.Positions[i] = 64f * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            var render = Vku.CreateGraphRender(graph);
            var forces = new Vector2[names.Length];
            while (!Vku.StepGraphRender(render))
            {
                Graph.StepEades(forces, graph.Positions, weights);
            }
            Vku.DestroyGraphRender(render);
        }

        public static (string[] names, float[,] weights) RunAnalysis(string projectPath, bool overwriteCache)
        {
            var formatter = new BinaryFormatter();
            if (File.Exists("project.bin") && !overwriteCache)
            {
                using var stream = File.OpenRead("project.bin");
                return ((string[], float[,]))formatter.Deserialize(stream);
            }
            else
            {
                MSBuildLocator.RegisterDefaults();
                var graph = Analyze.ConstructGraph(projectPath);
                if (graph.names.Length > 0)
                {
                    using var output = File.OpenWrite("project.bin");
                    formatter.Serialize(output, graph);
                }
                return graph;
            }
        }

        public static void AnalyzeProject(
            FileSystemInfo project,
            ProjectRenderKind kind,
            int iterationCount,
            bool overwriteCache,
            int every,
            long time)
        {
            var (names, weights) = RunAnalysis(project.FullName, overwriteCache);
            var state = Fdg.State.Create(weights);

            if (kind == ProjectRenderKind.Vku)
            {
                var render = Vku.CreateGraphRender(new InteropGraph(state.Positions, state.Weights));
                Vku.StepGraphRender(render);
                var stopwatch = new Stopwatch();
                var end = false;
                for (int i = 0; i < iterationCount && !end; ++i)
                {
                    Fdg.Step(ref state);
                    end = Vku.StepGraphRender(render);
                    Thread.Sleep((int)(time - stopwatch.ElapsedMilliseconds % time));
                }
                Vku.DestroyGraphRender(render);
            }
            else if (kind == ProjectRenderKind.Graphviz)
            {
                File.WriteAllText($"graph_000.gv", Graph.ToGraphviz(state.Positions, weights, names));
                for (int i = 0; i < iterationCount; ++i)
                {
                    Fdg.Step(ref state);
                    if (i % every == 0)
                    {
                        var current = i / every + 1;
                        File.WriteAllText($"graph_{current,000}.gv", Graph.ToGraphviz(state.Positions, weights, names));
                    }
                }
                var last = iterationCount / every + 1;
                File.WriteAllText($"graph_{last,000}.gv", Graph.ToGraphviz(state.Positions, weights, names));
            }
        }

        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand("A software visualization tool");
            rootCommand.AddCommand(new Command("triangle", "Draw a triangle")
            {
                Handler = CommandHandler.Create(DrawTriangle)
            });
            rootCommand.AddCommand(new Command("cube", "Draw a cube")
            {
                Handler = CommandHandler.Create(DrawCube)
            });
            rootCommand.AddCommand(new Command("tree", "Draw a tree")
            {
                Handler = CommandHandler.Create(DrawTree)
            });
            rootCommand.AddCommand(new Command("eades", "Draw an animation of Eades from cached project analysis")
            {
                Handler = CommandHandler.Create(DrawEades)
            });
            var projectCommand = new Command("project", "Analyze a project")
            {
                new Argument<FileSystemInfo>("project", "Path to an MSBuild project"),
                new Argument<ProjectRenderKind>("kind", "The way to display the results"),
                new Argument<int>("iterationCount", "How many steps should the FDG algorithm make"),
                new Option<bool>(new []{"-o", "--overwrite-cache"}, "Overwrite a cached project analysis"),
                new Option<int>("--every", () => 1000, "Generate a graphviz file every # steps (Graphviz kind only)"),
                new Option<long>("--time", () => 100, "Minimum time spent on one frame")
            };
            projectCommand.Handler = CommandHandler
                .Create<FileSystemInfo, ProjectRenderKind, int, bool, int, long>(AnalyzeProject);
            rootCommand.AddCommand(projectCommand);

            return rootCommand.Invoke(args);
        }
    }
}
