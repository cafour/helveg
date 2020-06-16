using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Numerics;
using Helveg.Landscape;
using Helveg.Render;

namespace Helveg
{
    public static class DebugDraw
    {
        public static void AddDrawCommands(Command parent)
        {
            parent.AddCommand(new Command("triangle", "Draw a triangle")
            {
                Handler = CommandHandler.Create(DrawTriangle)
            });
            parent.AddCommand(new Command("cube", "Draw a cube")
            {
                Handler = CommandHandler.Create(DrawCube)
            });
            parent.AddCommand(new Command("tree", "Draw a tree")
            {
                Handler = CommandHandler.Create(DrawTree)
            });
            parent.AddCommand(new Command("chunk", "Draw a single chunk")
            {
                Handler = CommandHandler.Create(DrawChunk)
            });
            parent.AddCommand(new Command("opensimplex", "Draw a single chunk with OpenSimplex noise")
            {
                Handler = CommandHandler.Create(DrawNoisyChunk)
            });
            parent.AddCommand(new Command("world", "Draw multiple noisy chunks")
            {
                Handler = CommandHandler.Create(DrawNoisyWorld)
            });
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

        private static void WriteSentence(IList<Spruce.Symbol> sentence)
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
    }
}
