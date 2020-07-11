using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Helveg.Analysis;
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

        public static Heightmap GenerateIslandHeightmap(
            IEnumerable<Vector2> positions,
            int radius,
            int padding = 50)
        {
            var (minX, minY) = ((int)(positions.Min(p => p.X) - padding), (int)(positions.Min(p => p.Y) - padding));
            var (maxX, maxY) = ((int)(positions.Max(p => p.X) + padding + 1), (int)(positions.Max(p => p.Y) + padding + 1));
            var heightmap = new Heightmap(minX, maxX, minY, maxY);
            foreach (var position in positions)
            {
                for (int x = -radius; x <= radius; ++x)
                {
                    for (int y = -radius; y <= radius; ++y)
                    {
                        var posX = (int)position.X + x;
                        var posY = (int)position.Y + y;
                        if (heightmap.TryGetValue(posX, posY, out _))
                        {
                            heightmap[posX, posY] += 36 * MathF.Exp(-4 * (x * x + y * y) / (float)(radius * radius));
                        }
                    }
                }
            }
            return heightmap;
        }

        public static WorldBuilder GenerateIsland(
            AnalyzedProject project,
            ImmutableDictionary<AnalyzedTypeId, Vector2> positions)
        {
            var heightmap = GenerateIslandHeightmap(positions.Values, 48, 96);
            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            for (int x = heightmap.MinX; x < heightmap.MaxX; ++x)
            {
                for (int z = heightmap.MinY; z < heightmap.MaxY; ++z)
                {
                    var y = (int)heightmap[x, z];
                    var surface = y <= 36
                        ? new Block { PaletteIndex = 2 }
                        : new Block { PaletteIndex = 1 };
                    var rockLevel = Math.Max(y - 4, 0);
                    world.FillLine(new Point3(x, 0, z), new Point3(x, rockLevel, z), new Block { PaletteIndex = 0 });
                    world.FillLine(new Point3(x, rockLevel, z), new Point3(x, y, z), surface);
                    if (y <= 32)
                    {
                        world.FillLine(new Point3(x, y + 1, z), new Point3(x, 32, z), new Block { PaletteIndex = 4 });
                    }
                }
            }

            foreach (var (id, position) in positions)
            {
                var type = project.Types[id];
                var center = new Point3(position.X, heightmap[position.X, position.Y], position.Y);
                var sentence = Spruce.Generate(
                    seed: project.Types[id].GetSeed(),
                    size: type.MemberCount);
                Spruce.Draw(
                    sentence: sentence,
                    world: world,
                    position: center,
                    wood: new Block { PaletteIndex = 3 },
                    needles: new Block { PaletteIndex = 5 },
                    hasNeedles: !type.Health.HasFlag(Diagnosis.Warning));
                if (type.Health.HasFlag(Diagnosis.Error))
                {
                    world.Burn(center + new Point3(0, 6, 0), MathF.Log2(type.MemberCount) * 2);
                }
            }
            return world;
        }
    }
}
