using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Numerics;
using Helveg.Landscape;
using Helveg.Render;
using Microsoft.Extensions.Logging;

namespace Helveg
{
    public static class DebugDraw
    {
        public static void AddDrawCommands(Command parent)
        {
            var vkDebugOpt = new Option<bool>(
                alias: "--vk-debug",
                description: "Enable/disable Vulkan validation layers",
                getDefaultValue: () => true);

            var triangleCmd = new Command("triangle", "Draw a triangle")
            {
                vkDebugOpt
            };
            triangleCmd.Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawTriangle))!);
            parent.AddCommand(triangleCmd);

            var cubeCmd = new Command("cube", "Draw a cube")
            {
                vkDebugOpt
            };
            cubeCmd.Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawCube))!);
            parent.AddCommand(cubeCmd);

            var treeCmd = new Command("tree", "Draw a tree")
            {
                vkDebugOpt
            };
            treeCmd.Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawTree))!);
            parent.AddCommand(treeCmd);

            var chunkCmd = new Command("chunk", "Draw a single chunk")
            {
                vkDebugOpt
            };
            chunkCmd.Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawChunk))!);
            parent.AddCommand(chunkCmd);

            var opensimplexCmd = new Command("opensimplex", "Draw a single chunk with OpenSimplex noise")
            {
                vkDebugOpt
            };
            opensimplexCmd.Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawNoisyChunk))!);
            parent.AddCommand(opensimplexCmd);

            var worldCmd = new Command("world", "Draw multiple noisy chunks")
            {
                vkDebugOpt
            };
            worldCmd.Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawNoisyWorld))!);
            parent.AddCommand(worldCmd);
        }

        public static void DrawTriangle(bool vkDebug)
        {
            Vku.SetDebug(vkDebug);
            Vku.HelloTriangle();
        }

        public static int DrawCube(bool vkDebug)
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
            Vku.SetDebug(vkDebug);
            return Vku.HelloMesh(mesh);
        }

        public static void DrawTree(bool vkDebug)
        {
            var logger = Program.Logging.CreateLogger("Debug Tree");
            var sentence = Tree.Generate(42, 12);
            logger.LogInformation(string.Concat(sentence));
            var worldBuilder = new WorldBuilder(64, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            Tree.Draw(sentence, worldBuilder, Point3.Zero, new Block { PaletteIndex = 3 }, new Block { PaletteIndex = 5 });
            Vku.SetDebug(vkDebug);
            Vku.HelloWorld(worldBuilder.Build());
        }

        public static void DrawChunk(bool vkDebug)
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
            Vku.SetDebug(vkDebug);
            Vku.HelloChunk(chunk);
        }

        public static void DrawNoisyChunk(bool vkDebug)
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
            Vku.SetDebug(vkDebug);
            Vku.HelloChunk(chunk);
        }

        public static void DrawNoisyWorld(bool vkDebug)
        {
            var world = Terrain.GenerateNoise(100).Build();
            Vku.SetDebug(vkDebug);
            Vku.HelloWorld(world);
        }
    }
}
