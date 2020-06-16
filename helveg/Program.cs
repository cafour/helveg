using System;
using System.Buffers;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;
using Helveg.Serialization;
using Helveg.Landscape;
using Helveg.Analysis;
using Helveg.Render;
using Microsoft.Build.Locator;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Helveg
{
    public class Program
    {
        public static ILoggerFactory Logging = new NullLoggerFactory();
        public static ILogger Logger = NullLogger.Instance;

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

        public static (string[] names, int[,] weights) RunAnalysis(string projectPath, bool overwriteCache)
        {
            var formatter = new BinaryFormatter();
            if (File.Exists("project.bin") && !overwriteCache)
            {
                using var stream = File.OpenRead("project.bin");
                return ((string[], int[,]))formatter.Deserialize(stream);
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

        public static async Task<AnalyzedProject> RunBetterAnalysis(string csprojPath)
        {
            const string cacheFile = "analysis.json";

            if (File.Exists(cacheFile))
            {
                using var stream = new FileStream(cacheFile, FileMode.Open);
                var project = await JsonSerializer.DeserializeAsync<SerializableProject>(stream);
                if (project.CsprojPath == csprojPath)
                {
                    return project.ToAnalyzed();
                }
            }

            MSBuildLocator.RegisterDefaults();
            var analyzedProject = await Analyze.AnalyzeProject(csprojPath);
            {
                using var stream = new FileStream(cacheFile, FileMode.Create);
                await JsonSerializer.SerializeAsync(
                    stream,
                    SerializableProject.FromAnalyzed(csprojPath, analyzedProject));
            }
            return analyzedProject;
        }

        public static void DrawGraphVku(
            Fdg.State state,
            int iterationCount,
            int noOverlapIterationCount,
            long time,
            float nodeSize)
        {
            var render = Vku.CreateGraphRender(new InteropGraph(state.Positions, state.Weights));
            Vku.StepGraphRender(render);
            var stopwatch = new Stopwatch();
            var end = false;
            state.NodeSize = nodeSize;
            // state.IsGravityStrong = true;
            // for (int i = 0; i < iterationCount && !end; ++i)
            // {
            //     Fdg.Step(ref state);
            //     end = Vku.StepGraphRender(render);
            //     Thread.Sleep((int)(time - stopwatch.ElapsedMilliseconds % time));
            // }
            state.IsGravityStrong = false;
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
            int[,] weights,
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
            long time,
            float nodeSize)
        {
            var (names, weights) = RunAnalysis(project.FullName, overwriteCache);
            var state = Fdg.State.Create(weights);

            switch (kind)
            {
                case ProjectRenderKind.Vku:
                    DrawGraphVku(state, iterationCount, noOverlapIterationCount, time, nodeSize);
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
                new Vector3(0.3f, 0.3f, 0.3f)
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
            var world = Terrain.GenerateNoise(100).Build();
            Vku.HelloWorld(world);
        }

        public static async Task DrawGraphWorld(FileSystemInfo project, bool overwriteCache)
        {
            const int iterationCount = 1000;
            const int noOverlapIterationCount = 800;
            var analyzedProject = await RunBetterAnalysis(project.FullName);
            var (names, weights) = analyzedProject.GetWeightMatrix();
            Vector2[] positions;
            if (File.Exists("positions.json") && !overwriteCache)
            {
                var positionsText = File.ReadAllText("positions.json");
                positions = System.Text.Json.JsonSerializer.Deserialize<Vector2[]>(positionsText, Serialize.JsonOptions);
            }
            else
            {
                var state = Fdg.State.Create(weights);
                state.NodeSize = 16;
                for (int i = 0; i < iterationCount; ++i)
                {
                    Fdg.Step(ref state);
                }
                state.IsGravityStrong = true;
                for (int i = 0; i < iterationCount; ++i)
                {
                    Fdg.Step(ref state);
                }
                state.IsGravityStrong = false;
                state.PreventOverlapping = true;
                for (int i = 0; i < noOverlapIterationCount; ++i)
                {
                    Fdg.Step(ref state);
                }
                var stream = new FileStream("positions.json", FileMode.Create);
                var writer = new Utf8JsonWriter(stream);
                System.Text.Json.JsonSerializer.Serialize(writer, state.Positions, Serialize.JsonOptions);
                positions = state.Positions;
            }

            var sizes = new List<int>();
            var seeds = new List<int>();
            foreach(var name in names)
            {
                var type = analyzedProject.Types[name];
                sizes.Add(type.Members.Length);
                seeds.Add(type.GetSeed());
            }

            var world = Terrain.GenerateIsland(positions, sizes.ToArray(), seeds.ToArray()).Build();
            foreach (var chunk in world.Chunks)
            {
                chunk.HollowOut(new Block { Flags = BlockFlags.IsAir });
            }
            Vku.HelloWorld(world);
        }

        public static unsafe int Main(string[] args)
        {
            Logging = LoggerFactory.Create(b => {
                b.AddConsole();
            });
            Logger = Logging.CreateLogger(string.Empty);

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
                new Option<long>("--time", () => 100, "Minimum time spent on one frame"),
                new Option<float>("--node-size", () => 1, "The size of nodes")
            };
            projectCommand.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(AnalyzeProject)));
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
