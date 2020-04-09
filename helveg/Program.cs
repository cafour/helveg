using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

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
            fixed (uint* indices = mesh.Indices)
            {
                return helloMesh(new Mesh.Raw
                {
                    Vertices = vertices,
                    Colors = colors,
                    Indices = indices,
                    VertexCount = (uint)mesh.Vertices.Length,
                    IndexCount = (uint)mesh.Indices.Length
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

            var indices = new uint[] {
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
                Console.WriteLine($"{prefix}Kind={symbol.Kind},Parameter={symbol.Parameter}");
                Console.WriteLine($"Sentence length: {sentence.Count}");
            }
        }

        public static void Main(string[] args)
        {
            var sentence = Spruce.Rewrite(new[]
                {
                    new Spruce.Symbol(Spruce.Kind.Trunk),
                    new Spruce.Symbol(Spruce.Kind.Canopy)
                },
                seed: 42,
                branchCount: 12,
                maxBranching: 6,
                minBranching: 3,
                initialBranching: 4,
                branchingDiff: 2);
            var spruceMesh = Spruce.GenerateMesh(sentence);
            Console.WriteLine($"Vertices length: {spruceMesh.Vertices.Length}");
            var mesh = HelloMesh(spruceMesh);
            Console.WriteLine($"Hello's return value: {mesh}");
        }
    }
}
