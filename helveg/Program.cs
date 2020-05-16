using System;
using System.Collections;
using System.Collections.Generic;
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
        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static extern int helloTriangle();

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int helloMesh(Mesh.Raw mesh);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int createGraphRender(InteropGraph.Raw graph, void** ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int stepGraphRender(void* ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int destroyGraphRender(void* ptr);

        public static unsafe InteropGraph.Raw CreateRawGraph(InteropGraph graph)
        {
            var raw = new InteropGraph.Raw
            {
                Positions = (Vector2*)Marshal.AllocHGlobal(sizeof(Vector2) * graph.Positions.Length).ToPointer(),
                Count = graph.Positions.Length
            };
            UpdateRawGraph(raw, graph);
            return raw;
        }

        public static unsafe void UpdateRawGraph(InteropGraph.Raw raw, InteropGraph graph)
        {
            if (graph.Positions.Length != raw.Count)
            {
                throw new ArgumentException("The updated graph must have the same amount of positions.");
            }
            var positionsSpan = new Span<Vector2>(raw.Positions, graph.Positions.Length);
            graph.Positions.CopyTo(positionsSpan);
        }

        public static unsafe void DestroyRawGraph(InteropGraph.Raw raw)
        {
            Marshal.FreeHGlobal(new IntPtr(raw.Positions));
        }

        public unsafe static int HelloMesh(Mesh mesh)
        {
            fixed (Vector3* vertices = mesh.Vertices)
            fixed (Vector3* colors = mesh.Colors)
            fixed (int* indices = mesh.Indices)
            {
                return helloMesh(new Mesh.Raw
                {
                    Vertices = vertices,
                    Colors = colors,
                    Indices = indices,
                    VertexCount = (int)mesh.Vertices.Length,
                    IndexCount = (int)mesh.Indices.Length
                });
            }
        }

        public static unsafe void HelloAnimatedGraph(string projectPath)
        {
            var (names, weights) = AnalyzeProject(projectPath);
            var graph = new InteropGraph(new Vector2[weights.GetLength(0)], Array.Empty<int>());
            for (int i = 0; i < graph.Positions.Length; ++i)
            {
                var angle = 2 * MathF.PI / graph.Positions.Length * i;
                graph.Positions[i] = 64f * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            fixed (Vector2* positions = graph.Positions)
            {
                var raw = new InteropGraph.Raw
                {
                    Positions = positions,
                    Count = graph.Positions.Length
                };
                Console.WriteLine(new IntPtr(positions).ToString("x"));
                void* graphRender = null;
                createGraphRender(raw, &graphRender);
                while (stepGraphRender(graphRender) == 0)
                {
                }
                destroyGraphRender(graphRender);
            }
            // var raw = CreateRawGraph(graph);
            // Console.WriteLine(new IntPtr(raw.Positions).ToString("x"));
            // void *graphRender = null;
            // createGraphRender(raw, &graphRender);
            // while (stepGraphRender(graphRender) == 0)
            // {
            // }
            // destroyGraphRender(graphRender);
            // DestroyRawGraph(raw);
            // File.WriteAllText($"forceatlas_00.gv", Graph.ToGraphviz(positions, graph, names));
            // Graph.RunForceAtlas2(positions, graph, 1000, 1000);
            // File.WriteAllText($"forceatlas_01.gv", Graph.ToGraphviz(positions, graph, names));
        }

        public static void HelloCube()
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

            var value = HelloMesh(mesh);
            Console.WriteLine($"Hello's return value: {value}");
        }

        public static (string[] names, float[,] graph) AnalyzeProject(string projectPath)
        {
            var formatter = new BinaryFormatter();
            if (File.Exists("project.bin"))
            {
                using var stream = File.OpenRead("project.bin");
                return ((string[] names, float[,] graph))formatter.Deserialize(stream);
            }
            MSBuildLocator.RegisterDefaults();
            var graph = Analyze.ConstructGraph(projectPath);
            using var output = File.OpenWrite("project.bin");
            formatter.Serialize(output, graph);
            return graph;
        }

        public static void DebugGraphForces(
            string name,
            string[] labels,
            Vector2[] positions,
            float[,] graph,
            int outer = 10,
            int inner = 100)
        {
            File.WriteAllText($"{name}_00.gv", Graph.ToGraphviz(positions, graph, labels));
            for (int i = 0; i < outer; ++i)
            {
                // Graph.ApplyForces(positions, graph, inner);
                // Graph.FruchtermanReingold(positions, graph, inner);
                File.WriteAllText($"{name}_{i + 1:00}.gv", Graph.ToGraphviz(positions, graph, labels));
            }
        }

        public static (string[] labels, Vector2[] positions) GetCircle(int count)
        {
            var labels = new string[count];
            var positions = new Vector2[count];
            for (int i = 0; i < count; ++i)
            {
                labels[i] = i.ToString();
                var angle = 2f * MathF.PI / count * i;
                positions[i] = count * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            return (labels, positions);
        }

        public static void HelloDebugGraph()
        {
            var (labels, positions) = GetCircle(4);
            var weights = new float[4, 4]
            {
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 1, 0}
            };
            DebugGraphForces("graph", labels, positions, weights, 10, 1000);
        }

        public static void HelloProject(string projectPath)
        {
            // var (names, graph) = AnalyzeProject(projectPath);
            // var positions = new Vector2[graph.GetLength(0)];
            // for (int i = 0; i < graph.GetLength(0); ++i)
            // {
            //     var angle = 2 * MathF.PI / graph.GetLength(0) * i;
            //     positions[i] = 64f * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            // }
            var positions = new Vector2[] { new Vector2(0, 0) };
            // File.WriteAllText($"forceatlas_00.gv", Graph.ToGraphviz(positions, graph, names));
            // Graph.RunForceAtlas2(positions, graph, 1000, 1000);
            // File.WriteAllText($"forceatlas_01.gv", Graph.ToGraphviz(positions, graph, names));
            var mesh = Graph.ToMesh(positions);
            HelloMesh(mesh);
        }

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

        public static void Main(string[] args)
        {
            // HelloProject(args[0]);
            HelloAnimatedGraph(args[0]);
            // HelloDebugGraph();
            // var sentence = Spruce.Rewrite(new[]
            //     {
            //         new Spruce.Symbol(Spruce.Kind.Canopy)
            //     },
            //     seed: 42,
            //     branchCount: 12,
            //     maxBranching: 6,
            //     minBranching: 3,
            //     initialBranching: 4,
            //     branchingDiff: 2);
            // var spruceMesh = Spruce.GenerateMesh(sentence);
            // WriteSentence(sentence);
            // Console.WriteLine($"Vertices length: {spruceMesh.Vertices.Length}");
            // var mesh = HelloMesh(spruceMesh);
            // Console.WriteLine($"Hello's return value: {mesh}");
        }
    }
}
