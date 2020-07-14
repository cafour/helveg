using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Helveg.Analysis;
using Helveg.Render;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
            int seed,
            IEnumerable<Vector2> positions,
            int radius,
            int padding = 50)
        {
            var (minX, minY) = ((int)(positions.Min(p => p.X) - padding), (int)(positions.Min(p => p.Y) - padding));
            var (maxX, maxY) = ((int)(positions.Max(p => p.X) + padding + 1), (int)(positions.Max(p => p.Y) + padding + 1));
            var heightmap = new Heightmap(minX, maxX, minY, maxY);
            const double frequency = 0.05;
            const double magnitude = 4.0;
            var openSimplex = new OpenSimplexNoise.Data(seed);
            Parallel.For(0, heightmap.SizeX * heightmap.SizeY, index =>
            {
                var x = index % heightmap.SizeX + heightmap.MinX;
                var y = index / heightmap.SizeX + heightmap.MinY;
                var coord = new Vector2(x, y);
                var sum = OpenSimplexNoise.Evaluate(
                    openSimplex,
                    x * frequency, y * frequency) * magnitude;
                foreach (var position in positions)
                {
                    var l = (position - coord).LengthSquared();
                    sum += 36 * Math.Exp(-4 * l / (radius * radius));
                }
                heightmap[x, y] = (int)Math.Round(sum);
            });
            return heightmap;
        }

        public static WorldBuilder GenerateIsland(
            AnalyzedProject project,
            ImmutableDictionary<AnalyzedTypeId, Vector2> positions,
            ILogger? logger = null)
        {
            const int meanSurfaceLevel = 34;
            const double surfaceNoiseMagnitude = 4.0;
            const double surfaceNoiseFrequency = 0.5;

            logger ??= NullLogger.Instance;
            logger.LogInformation($"Computing heightmap for '{project.Name}'.");
            var heightmap = GenerateIslandHeightmap(project.GetSeed(), positions.Values, 48, 96);

            logger.LogInformation($"Generating terrain for '{project.Name}'.");
            var world = new WorldBuilder(128, new Block { Flags = BlockFlags.IsAir }, Colours.IslandPalette);
            var openSimplex = new OpenSimplexNoise.Data(project.GetSeed());
            Parallel.For(0, heightmap.SizeX * heightmap.SizeY, index =>
            {
                var x = index % heightmap.SizeX + heightmap.MinX;
                var y = index / heightmap.SizeX + heightmap.MinY;
                var height = (int)heightmap[x, y];
                var surfaceNoise = OpenSimplexNoise.Evaluate(
                    openSimplex, x * surfaceNoiseFrequency,
                    y * surfaceNoiseFrequency);
                var surfaceLevel = meanSurfaceLevel + (int)Math.Round(surfaceNoise * surfaceNoiseMagnitude);
                var surface = height <= surfaceLevel
                    ? new Block { PaletteIndex = 2 }
                    : new Block { PaletteIndex = 1 };
                var rockLevel = Math.Max(height - 4, 1);
                world.FillLine(new Point3(x, 0, y), new Point3(x, rockLevel, y), new Block { PaletteIndex = 0 });
                world.FillLine(new Point3(x, rockLevel, y), new Point3(x, height, y), surface);
                // if (height <= 32)
                // {
                //     world.FillLine(new Point3(x, height + 1, y), new Point3(x, 32, y), new Block { PaletteIndex = 4 });
                // }
            });

            logger.LogInformation($"Generating structures for '{project.Name}'.");
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
