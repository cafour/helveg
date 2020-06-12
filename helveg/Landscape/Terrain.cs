using System;
using System.Collections.Generic;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Terrain
    {
        public static WorldBuilder GenerateNoise(int radius)
        {
            var palette = new Vector3[] {
                new Vector3(0.8f, 0.8f, 0.8f)
            };
            var openSimplex = new OpenSimplexNoise.Data(469242);
            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, palette);
            for (int x = -radius; x <= radius; ++x)
            {
                for (int z = -radius; z <= radius; ++z)
                {
                    var y = (int)(OpenSimplexNoise.Evaluate(openSimplex, x * 0.025, z * 0.025) * 32 + 32);
                    world[x, y, z] = new Block { PaletteIndex = 0 };
                }
            }
            return world;
        }

        public static WorldBuilder GenerateIsland(
            IEnumerable<Vector2> positions,
            long seed = 42,
            double frequency = 0.025,
            int radius = 4,
            int heightUnit = 1,
            int waterLevel = 32)
        {
            var palette = new Vector3[] {
                new Vector3(0.8f, 0.8f, 0.8f)
            };
            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, palette);
            foreach (var position in positions)
            {
                for (int x = -radius; x <= radius; ++x)
                {
                    for (int z = -radius; z <= radius; ++z)
                    {
                        var distance = MathF.Sqrt(x * x + z * z);
                        var current = new Point3(position.X + x, 0, position.Y);
                        world[current] = new Block
                        {
                            // PaletteIndex = (byte)(radius - (int)distance)
                            PaletteIndex = 0
                        };
                    }
                }
            }

            var noise = new OpenSimplexNoise.Data(seed);
            var (min, max) = world.GetBoundingBox();
            for (int x = min.X; x < max.X; ++x)
            {
                for (int z = min.Z; z < max.Z; ++z)
                {
                    // var block = world[x, 0, z];
                    // world[x, 0, z] = new Block{Flags = BlockFlags.IsAir};
                    var value = (OpenSimplexNoise.Evaluate(noise, x * frequency, z * frequency) + 1) * 16;
                    // + block.PaletteIndex * heightUnit;
                    world.FillLine(new Point3(x, 0, z), new Point3(x, (int)value, z), new Block { PaletteIndex = 0 });
                }
            }

            return world;
        }
    }
}
