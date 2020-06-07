using System;
using System.Collections.Generic;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Terrain
    {
        public static WorldBuilder GenerateIsland(
            IEnumerable<Vector2> positions,
            long seed = 42,
            double frequency = 0.025,
            int radius = 4,
            int heightUnit = 4,
            int waterLevel = 32)
        {
            var palette = new Vector3[] {
                new Vector3(0.8f, 0.8f, 0.8f)
            };
            var world = new WorldBuilder(64, new Block { Flags = BlockFlags.IsAir }, palette);
            foreach (var position in positions)
            {
                for (int x = -radius; x <= radius; ++x)
                {
                    for (int z = -radius; z <= radius; ++z)
                    {
                        var distance = MathF.Sqrt(x * x + z * z);
                        var current = new Vector3(position.X + x, 0, position.Y);
                        world.SetBlock(current, new Block
                        {
                            PaletteIndex = (byte)(radius - (int)distance)
                        });
                    }
                }
            }

            var noise = new OpenSimplexNoise.Data(seed);
            var (min, max) = world.GetBoundingBox();
            for(int x = (int)min.X; x < max.X; ++x)
            {
                for (int z = (int)min.Z; z < max.Z; ++z)
                {
                    var block = world.GetBlock(new Vector3(x, 0, z));
                    var value = OpenSimplexNoise.Evaluate(noise, x * frequency, z * frequency) * 32
                        + block.PaletteIndex * heightUnit;
                    world.FillColumnTo(new Vector2(x, z), new Block {PaletteIndex = 0}, (int)value);
                }
            }

            return world;
        }
    }
}
