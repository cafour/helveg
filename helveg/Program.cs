using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Build.Locator;

namespace Helveg
{
    public class Program
    {
        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static extern int helloTriangle();

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int helloMesh(Mesh.Raw mesh);

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

        public static void HelloGraph()
        {
            var random = new Random(42);
            var positions = new Vector2[42];
            var weights = new float[42, 42];
            var labels = new string[42];
            for (int i = 0; i < 42; ++i)
            {
                labels[i] = i.ToString();
                var angle = 2 * MathF.PI / 42f * i;
                positions[i] = 10 * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                for (int j = 0; j < 42; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    var weight = random.Next(0, 42);
                    weights[i, j] = weight < 32 ? 0 : (weight - 32f) * 2;
                }
            }

            Graph.ApplyForces(positions, weights, 10000);
            File.WriteAllText("test.gv", Graph.Dotify(positions, weights, labels));
        }

        public static void HelloCircle()
        {
            var positions = new Vector2[11];
            var weights = new float[11, 11];
            var labels = new string[11];
            for (int i = 0; i < 10; ++i)
            {
                labels[i] = i.ToString();
                var angle = 2f * MathF.PI / 10f * i;
                positions[i] = 100 * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                weights[i, (i + 1) % 10] = 1;
                weights[i, (i + 9) % 10] = 1;
            }
            positions[10] = new Vector2(128, 128);
            labels[10] = "fuck";
            weights[10, 0] = 2;
            for (int i = 0; i < 10; ++i)
            {
                Graph.ApplyForces(positions, weights, 100);
                File.WriteAllText($"circle_{i}.gv", Graph.Dotify(positions, weights, labels));
            }
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

        public static void HelloProject(string projectPath)
        {
            var (names, graph) = AnalyzeProject(projectPath);
            var positions = new Vector2[graph.GetLength(0)];
            for (int i = 0; i < graph.GetLength(0); ++i)
            {
                var angle = 2 * MathF.PI / graph.GetLength(0) * i;
                positions[i] = MathF.Log2(graph.GetLength(0)) * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            for (int i = 0; i < 10; ++i)
            {
                File.WriteAllText($"project_{i:00}.gv", Graph.Dotify(positions, graph, names));
                Graph.ApplyForces(positions, graph, 100);
            }
            File.WriteAllText($"project_10.gv", Graph.Dotify(positions, graph, names));
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
            HelloProject(args[0]);
            // HelloCircle();
            // HelloGraph();
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
