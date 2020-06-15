using System;
using System.Collections.Generic;
using System.Linq;
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
                        var e = MathF.Sqrt(x * x + y * y) / radius;
                        heightmap[posX, posY] += 36 * MathF.Exp(-(e * e));
                    }
                }
            }
            return heightmap;
        }

        public static WorldBuilder GenerateIsland(Vector2[] positions, int[] sizes, int[] seeds)
        {
            var heightmap = GenerateIslandHeightmap(positions, 16);
            var palette = new Vector3[] {
                new Vector3(146, 146, 146) / 255, // stone
                new Vector3(109, 182, 73) / 255, // grass
                new Vector3(219, 182, 146) / 255, // sand
                new Vector3(109, 73, 36) / 255, // wood
                new Vector3(0, 146, 219) / 255 // water
            };
            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, palette);
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

            for (int i = 0; i < positions.Length; ++i)
            {
                var center = new Point3(positions[i].X, heightmap[positions[i].X, positions[i].Y], positions[i].Y);
                var sentence = Spruce.Rewrite(
                    axiom: new[] { new Spruce.Symbol(Spruce.Kind.Canopy) },
                    seed: seeds[i],
                    branchCount: Math.Clamp(sizes[i], 0, 9),
                    maxBranching: 6,
                    minBranching: 3,
                    initialBranching: 4,
                    branchingDiff: 2);
                Spruce.PlaceStructure(world, center, new Block { PaletteIndex = 3 }, sentence);
            }
            return world;
        }
    }
}
