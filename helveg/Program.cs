using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;
using Helveg.Serialization;
using Helveg.Landscape;
using Helveg.Analysis;
using Helveg.Render;
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
                    prefix = prefix[0..^1];
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

        public static void DrawGraphVku(Fdg.State state, int iterationCount, int noOverlapIterationCount, long time)
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
            state.PreventOverlapping = true;
            for (int i = 0; i < noOverlapIterationCount && !end; ++i)
            {
                Fdg.Step(ref state);
                end = Vku.StepGraphRender(render);
                Thread.Sleep((int)(time - stopwatch.ElapsedMilliseconds % time));
            }
            Vku.DestroyGraphRender(render);
        }

        public static void DrawGraphGraphviz(
            Fdg.State state,
            string[] names,
            float[,] weights,
            int iterationCount,
            int noOverlapIterationCount,
            int every)
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
            state.PreventOverlapping = true;
            for (int i = 0; i < noOverlapIterationCount; ++i)
            {
                Fdg.Step(ref state);
                if ((iterationCount + i) % every == 0)
                {
                    var current = (iterationCount + i) / every + 1;
                    File.WriteAllText($"graph_{current,000}.gv", Graph.ToGraphviz(state.Positions, weights, names));
                }
            }
            var last = (iterationCount + noOverlapIterationCount) / every + 1;
            File.WriteAllText($"graph_{last,000}.gv", Graph.ToGraphviz(state.Positions, weights, names));
        }

        public static void AnalyzeProject(
            FileSystemInfo project,
            ProjectRenderKind kind,
            int iterationCount,
            int noOverlapIterationCount,
            bool overwriteCache,
            int every,
            long time)
        {
            var (names, weights) = RunAnalysis(project.FullName, overwriteCache);
            var state = Fdg.State.Create(weights);

            switch (kind)
            {
                case ProjectRenderKind.Vku:
                    DrawGraphVku(state, iterationCount, noOverlapIterationCount, time);
                    break;
                case ProjectRenderKind.Graphviz:
                    DrawGraphGraphviz(state, names, weights, iterationCount, noOverlapIterationCount, every);
                    break;
                default:
                    throw new ArgumentException($"ProjectRenderKind '{kind}' is not supported.");
            }
        }

        public static void DrawChunk()
        {
            var blockTypes = new Block[]
            {
                new Block {Flags = BlockFlags.IsAir},
                new Block {PaletteIndex = 0},
                new Block {PaletteIndex = 1}
            };
            var palette = new[]
            {
                new Vector3(0.5f, 0.3f, 0.0f),
                new Vector3(0.0f, 0.3f, 0.5f)
            };
            var size = 16;
            var voxels = new Block[size, size, size];
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int z = 0; z < size; ++z)
                    {
                        voxels[x, y, z] = blockTypes[(x + y + z) % blockTypes.Length];
                    }
                }
            }
            var chunk = new Chunk(voxels, palette);
            Vku.HelloChunk(chunk);
        }

        public static void DrawNoisyChunk()
        {
            var palette = new[]
            {
                new Vector3(0.3f, 0.3f, 0.3f),
            };

            var openSimplex = new OpenSimplexNoise.Data(42L);
            var chunk = Chunk.CreateNoisy(
                size: 64,
                palette: palette,
                stoneIndex: 0,
                openSimplex: openSimplex,
                frequency: 0.025,
                offset: Vector2.Zero);
            Vku.HelloChunk(chunk);
        }

        public static void DrawNoisyWorld()
        {
            var palette = new[]
            {
                new Vector3(0.3f, 0.3f, 0.3f),
            };
            const int chunkSize = 64;
            var openSimplex = new OpenSimplexNoise.Data(42L);
            var chunks = new List<Chunk>();
            var positions = new List<Vector3>();
            var width = 5;
            var height = 5;
            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < height; ++z)
                {
                    positions.Add(chunkSize * new Vector3(x, 0.0f, z));
                    var chunk = Chunk.CreateNoisy(
                        size: chunkSize,
                        palette: palette,
                        stoneIndex: 0,
                        openSimplex: openSimplex,
                        frequency: 0.025,
                        offset: new Vector2(x, z) * chunkSize);
                    chunks.Add(chunk);
                }
            }

            var world = new World(chunkSize, chunks.ToArray(), positions.ToArray());
            Vku.HelloWorld(world);
        }

        public static void DrawGraphWorld(FileSystemInfo project, bool overwriteCache)
        {
            const int iterationCount = 200;
            const int noOverlapIterationCount = 400;
            const int chunkSize = 64;
            var (names, weights) = RunAnalysis(project.FullName, overwriteCache);
            var positions = Array.Empty<Vector2>();
            if (File.Exists("positions.json") && !overwriteCache)
            {
                var positionsText = File.ReadAllText("positions.json");
                positions = JsonSerializer.Deserialize<Vector2[]>(positionsText, Serialize.JsonOptions);
            }
            else
            {
                var state = Fdg.State.Create(weights);
                for (int i = 0; i < iterationCount; ++i)
                {
                    Fdg.Step(ref state);
                }
                state.PreventOverlapping = true;
                for (int i = 0; i < noOverlapIterationCount; ++i)
                {
                    Fdg.Step(ref state);
                }
                var stream = new FileStream("positions.json", FileMode.Create);
                var writer = new Utf8JsonWriter(stream);
                JsonSerializer.Serialize(writer, state.Positions, Serialize.JsonOptions);
                positions = state.Positions;
            }

            // var builder = ImmutableDictionary.CreateBuilder<Vector3, Chunk>();
            // for (float x = boxMin.X; x < boxMax.X; x += chunkSize)
            // {
            //     for (float z = boxMin.Y; z < boxMax.Y; z += chunkSize)
            //     {
            //         builder.Add(new Vector3(x, 0, z), Chunk.CreateHorizontalPlane(
            //             size: chunkSize,
            //             palette: palette,
            //             planeBlock: new Block { PaletteIndex = 0 }));
            //     }
            // }
            // var world = new World(chunkSize, builder.ToImmutable());
            var world = Terrain.GenerateIsland(positions).Build();
            Vku.HelloWorld(world);
        }

        public static unsafe int Main(string[] args)
        {
            var rootCommand = new RootCommand("A software visualization tool")
            {
                new Argument<FileSystemInfo>("project", "Path to an MSBuild project"),
                new Option<bool>(new []{"-o", "--overwrite-cache"}, "Overwrite a cached project analysis")
            };
            rootCommand.Handler = CommandHandler.Create<FileSystemInfo, bool>(DrawGraphWorld);
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
            var projectCommand = new Command("graph", "Analyze a project")
            {
                new Argument<FileSystemInfo>("project", "Path to an MSBuild project"),
                new Argument<ProjectRenderKind>("kind", "The way to display the results"),
                new Argument<int>("iterationCount", "Step count"),
                new Argument<int>("noOverlapIterationCount", "Overlap-preventing step count"),
                new Option<bool>(new []{"-o", "--overwrite-cache"}, "Overwrite a cached project analysis"),
                new Option<int>("--every", () => 1000, "Generate a graphviz file every # steps (Graphviz kind only)"),
                new Option<long>("--time", () => 100, "Minimum time spent on one frame")
            };
            projectCommand.Handler = CommandHandler
                .Create<FileSystemInfo, ProjectRenderKind, int, int, bool, int, long>(AnalyzeProject);
            rootCommand.AddCommand(projectCommand);
            rootCommand.AddCommand(new Command("chunk", "Draw a single chunk")
            {
                Handler = CommandHandler.Create(DrawChunk)
            });
            rootCommand.AddCommand(new Command("opensimplex", "Draw a single chunk with OpenSimplex noise")
            {
                Handler = CommandHandler.Create(DrawNoisyChunk)
            });
            rootCommand.AddCommand(new Command("world", "Draw multiple noisy chunks")
            {
                Handler = CommandHandler.Create(DrawNoisyWorld)
            });

            return rootCommand.Invoke(args);
        }
    }
}
