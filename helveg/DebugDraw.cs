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

            var triangleCmd = new Command("triangle", "Draw a triangle")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawTriangle))!)
            };
            parent.AddCommand(triangleCmd);

            var cubeCmd = new Command("cube", "Draw a cube")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawCube))!)
            };
            parent.AddCommand(cubeCmd);

            var treeCmd = new Command("tree", "Draw a tree")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawTree))!)
            };
            parent.AddCommand(treeCmd);

            var cabinCmd = new Command("cabin", "Draw a log cabin")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawCabin))!)
            };
            parent.AddCommand(cabinCmd);

            var cargoCmd = new Command("cargo", "Draw some cargo")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawCargo))!)
            };
            parent.AddCommand(cargoCmd);

            var bridgeCmd = new Command("bridge", "Draw a bridge")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawBridge))!)
            };
            parent.AddCommand(bridgeCmd);

            var chunkCmd = new Command("chunk", "Draw a single chunk")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawChunk))!)
            };
            parent.AddCommand(chunkCmd);

            var opensimplexCmd = new Command("opensimplex", "Draw a single chunk with OpenSimplex noise")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawNoisyChunk))!)
            };
            parent.AddCommand(opensimplexCmd);

            var worldCmd = new Command("world", "Draw multiple noisy chunks")
            {
                Handler = CommandHandler.Create(typeof(DebugDraw).GetMethod(nameof(DrawNoisyWorld))!)
            };
            parent.AddCommand(worldCmd);
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

        public static void DrawTree()
        {
            var logger = Program.Logging.CreateLogger("Debug Tree");
            var sentence = Spruce.Generate(42, 15);
            logger.LogInformation(string.Concat(sentence));
            var worldBuilder = new WorldBuilder(64, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            const int tintCount = 6;
            for (int i = 0; i < tintCount; ++i)
            {
                for (int j = i; j < tintCount; ++j)
                {
                    Spruce.Draw(
                    sentence,
                    worldBuilder,
                    new Point3(i * 20, 0, j * 20),
                    new Block(Colours.Island.Wood),
                    new Block { PaletteIndex = (byte)((int)Colours.Island.Needles0 + (i + j) % tintCount) });
                }
            }
            var world = worldBuilder.Build();
            Vku.HelloWorld(world);
        }

        public static void DrawCabin()
        {
            var worldBuilder = new WorldBuilder(64, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            LogCabin.Draw(
                worldBuilder,
                Point3.Zero,
                new Block(Colours.Island.Wood0),
                new Block(Colours.Island.Wood1),
                new Block(Colours.Island.Roof),
                6,
                4);
            var world = worldBuilder.Build();
            Vku.HelloWorld(world);
        }

        public static void DrawCargo()
        {
            var worldBuilder = new WorldBuilder(64, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            Cargo.Draw(
                world: worldBuilder,
                position: new Point3(0, 32, 0),
                platform: new Block(Colours.Island.Wood),
                corner: new Block(Colours.Island.Stone),
                containers: new Block[]
                {
                    new Block(Colours.Island.Cargo0),
                    new Block(Colours.Island.Cargo1),
                    new Block(Colours.Island.Cargo2)
                },
                seed: 42,
                containerCount: 5);
            var world = worldBuilder.Build();
            Vku.HelloWorld(world);
        }

        public static void DrawBridge()
        {
            var worldBuilder = new WorldBuilder(64, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            Bridge.Draw(
                world: worldBuilder,
                from: Point3.Zero,
                to: new Point3(100, 0, 0),
                bridge: new [] {new Block(Colours.Island.Cargo0), new Block(Colours.Island.Cargo1)},
                height: 8);
            var world = worldBuilder.Build();
            Vku.HelloWorld(world);
        }

        public static void DrawChunk()
        {
            var blockTypes = new Block[]
            {
                new Block {Flags = BlockFlags.IsAir},
                new Block(Colours.Island.Stone),
                new Block(Colours.Island.Grass)
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

            var chunk = Chunk.CreateNoisy(
                size: 64,
                palette: palette,
                fill: new Block { PaletteIndex = 0 },
                seed: 42,
                frequency: 0.025,
                offset: Vector2.Zero);
            Vku.HelloChunk(chunk);
        }

        public static void DrawNoisyWorld()
        {
            const int radius = 100;
            var palette = new Vector3[] {
                new Vector3(0.8f, 0.8f, 0.8f)
            };
            var noise = OpenSimplex.Create(469242);
            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, palette);
            for (int x = -radius; x <= radius; ++x)
            {
                for (int z = -radius; z <= radius; ++z)
                {
                    var y = (int)(noise.Evaluate(x * 0.025, z * 0.025) * 32 + 32);
                    world[x, y, z] = new Block(Colours.Island.Stone);
                }
            }
            var builtWorld = world.Build();
            Vku.HelloWorld(builtWorld);
        }
    }
}
