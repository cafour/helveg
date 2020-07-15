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
        public const int WaterLevel = 32;
        public const int SandLevel = 36;
        public const int GrassLevel = 40;
        public const float NodeOverlap = 8f;
        public const float StepWidth = 16f;

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
            Vector2[] positions,
            float[] sizes,
            int padding = 50)
        {
            var (minX, minY) = ((int)(positions.Min(p => p.X) - padding), (int)(positions.Min(p => p.Y) - padding));
            var (maxX, maxY) = ((int)(positions.Max(p => p.X) + padding + 1), (int)(positions.Max(p => p.Y) + padding + 1));
            var heightmap = new Heightmap(minX, maxX, minY, maxY);
            const double frequency = 0.05;
            const float magnitude = 4.0f;
            var openSimplex = new OpenSimplexNoise.Data(seed);
            Parallel.For(0, heightmap.SizeX * heightmap.SizeY, index =>
            {
                var x = index % heightmap.SizeX + heightmap.MinX;
                var y = index / heightmap.SizeX + heightmap.MinY;
                var coord = new Vector2(x, y);
                var sum = 0.0f;
                for (int i = 0; i < positions.Length; ++i)
                {
                    var l = (positions[i] - coord).Length();
                    var r = sizes[i] + NodeOverlap;
                    sum = MathF.Max(sum, GrassLevel * (1.0f - Smoothstep(0.0f, StepWidth, l - r)));
                }
                sum += (float)OpenSimplexNoise.Evaluate(
                    openSimplex,
                    x * frequency, y * frequency) * magnitude;
                heightmap[x, y] = (int)Math.Max(0.0f, Math.Round(sum));
            });
            return heightmap;
        }

        public static void GenerateIsland(
            WorldBuilder world,
            AnalyzedProject project,
            Vector2[] positions,
            float[] sizes,
            AnalyzedTypeId[] ids,
            (int x, int y) offset,
            ILogger? logger = null)
        {
            const double surfaceNoiseMagnitude = 4.0;
            const double surfaceNoiseFrequency = 0.5;

            logger ??= NullLogger.Instance;
            logger.LogInformation($"Computing heightmap.");
            var heightmap = GenerateIslandHeightmap(project.GetSeed(), positions, sizes);

            logger.LogInformation($"Generating terrain.");
            var openSimplex = new OpenSimplexNoise.Data(project.GetSeed());
            Parallel.For(0, heightmap.SizeX * heightmap.SizeY, index =>
            {
                var x = index % heightmap.SizeX + heightmap.MinX;
                var y = index / heightmap.SizeX + heightmap.MinY;
                var height = (int)heightmap[x, y];
                var surfaceNoise = OpenSimplexNoise.Evaluate(
                    openSimplex, x * surfaceNoiseFrequency,
                    y * surfaceNoiseFrequency);
                var currentSandLevel = SandLevel + (int)Math.Round(surfaceNoise * surfaceNoiseMagnitude);
                var surface = height <= currentSandLevel
                    ? new Block { PaletteIndex = 2 }
                    : new Block { PaletteIndex = 1 };
                var rockLevel = Math.Max(height - 4, 1);
                var ox = x + offset.x;
                var oy = y + offset.y;
                world.FillLine(new Point3(ox, 0, oy), new Point3(ox, rockLevel, oy), new Block { PaletteIndex = 0 });
                world.FillLine(new Point3(ox, rockLevel, oy), new Point3(ox, height, oy), surface);
                if (height < WaterLevel)
                {
                    world.FillLine(new Point3(ox, height + 1, oy), new Point3(ox, 32, oy), new Block { PaletteIndex = 4 });
                }
            });

            logger.LogInformation($"Generating structures.");
            for (int i = 0; i < positions.Length; ++i)
            {
                var type = project.Types[ids[i]];
                var position = positions[i];
                var center = new Point3(position.X + offset.x, heightmap[position.X, position.Y], position.Y + offset.y);
                var sentence = Spruce.Generate(
                    seed: type.GetSeed(),
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
        }

        private static float Smoothstep(float low, float high, float value)
        {
            // https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/smoothstep.xhtml
            value = Math.Clamp((value - low) / (high - low), 0.0f, 1.0f);
            return value * value * (3.0f - 2.0f * value);
        }
    }
}
