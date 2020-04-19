using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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
            var weights = new float[42,42];
            for (int i = 0; i < 42; ++i)
            {
                var angle = 2 * MathF.PI / 42f * i;
                positions[i] = 10 * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                for (int j = 0; j < 42; ++j)
                {
                    if (i == j) {
                        continue;
                    }
                    var weight = random.Next(0, 42);
                    weights[i,j] = weight < 32 ? 0 : (weight - 32f) * 2;
                }
            }

            positions = Graph.ApplyForces(positions, weights, 10000);
            File.WriteAllText("test.gv", Graph.Dotify(positions, weights));
        }

        public static void HelloProject(string projectPath)
        {
            var test = MSBuildLocator.QueryVisualStudioInstances();
            MSBuildLocator.RegisterDefaults();
            var graph = Analyse.ConstructGraph(projectPath);
            var positions = new Vector2[graph.GetLength(0)];
            for (int i = 0; i < graph.GetLength(0); ++i)
            {
                var angle = 2 * MathF.PI / graph.GetLength(0) * i;
                positions[i] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            positions = Graph.ApplyForces(positions, graph, 500);
            File.WriteAllText("project.gv", Graph.Dotify(positions, graph));
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
