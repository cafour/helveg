using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Helveg.Analysis;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Terrain
    {
        public const int OceanFloorLevel = 4;
        public const int WaterLevel = 32;
        public const int SandLevel = 36;
        public const int GrassLevel = 40;
        public const double OceanFloorNoiseFrequency = 0.05;
        public const double IslandNoiseFrequency = 0.05;
        public const double IslandNoiseMagnitude = 4.0;
        public const double GlobalNoiseFrequency = 0.5;
        public const double GlobalNoiseMagnitude = 3.0;
        public const float NodeOverlap = 8f;
        public const float SlopeWidth = 16f;

        public static void WriteOceanFloorHeightmap(Heightmap heightmap, long seed)
        {
            var noise = OpenSimplex.Create(seed);
            Parallel.For(heightmap.MinY, heightmap.MaxY, y =>
            {
                for (int x = heightmap.MinX; x < heightmap.MaxX; ++x)
                {
                    var noiseValue = (noise.Evaluate(x * OceanFloorNoiseFrequency, y * OceanFloorNoiseFrequency) + 1.0)
                        * OceanFloorLevel / 2.0;
                    heightmap[x, y] = (float)noiseValue;
                }
            });
        }

        public static void WriteIslandHeightmap(
            Heightmap heightmap,
            Rectangle area,
            long seed,
            Vector2[] positions,
            float[] sizes)
        {
            var center = new Vector2((area.X + area.Width) / 2, (area.Y + area.Height) / 2);
            var noise = OpenSimplex.Create(seed);
            Parallel.For(area.Y, area.Y + area.Height, y =>
            {
                for (int x = area.X; x < area.X + area.Width; ++x)
                {
                    var coord = new Vector2(x, y);
                    var sum = 0.0f;
                    for (int i = 0; i < positions.Length; ++i)
                    {
                        var l = (positions[i] - coord).Length();
                        var r = sizes[i] + NodeOverlap;
                        sum = MathF.Max(sum, GrassLevel * (1.0f - Smoothstep(0.0f, SlopeWidth, l - r)));
                    }
                    var noiseValue = (noise.Evaluate(x * IslandNoiseFrequency, y * IslandNoiseFrequency) + 1.0)
                        * IslandNoiseMagnitude / 2.0;
                    sum += (float)noiseValue;
                    heightmap[x, y] = MathF.Max(heightmap[x, y], sum);
                }
            });
        }

        public static void GenerateTerrain(Heightmap heightmap, WorldBuilder world)
        {
            var noise = OpenSimplex.Create(ISeedable.Arbitrary);
            Parallel.For(heightmap.MinY, heightmap.MaxY, y =>
            {
                for(int x = heightmap.MinX; x < heightmap.MaxX; ++x)
                {
                    var height = (int)MathF.Round(heightmap[x, y]);
                    var noiseValue = (int)(noise.Evaluate(
                        x * GlobalNoiseFrequency,
                        y * GlobalNoiseFrequency) * GlobalNoiseMagnitude);
                    var rockLevel = Math.Max(height - 4, 1);
                    world.FillLine(
                        new Point3(x, 0, y),
                        new Point3(x, rockLevel, y),
                        new Block(Colours.Island.Stone));
                    var surface = height < SandLevel + noiseValue
                        ? new Block(Colours.Island.Sand)
                        : new Block(Colours.Island.Grass);
                    world.FillLine(
                        new Point3(x, rockLevel, y),
                        new Point3(x, height, y),
                        surface);
                    if (height < WaterLevel)
                    {
                        world.FillLine(
                            new Point3(x, height + 1, y),
                            new Point3(x, WaterLevel, y),
                            new Block(Colours.Island.Water));
                    }
                }
            });
        }

        public static void PlaceStructures(
            Heightmap heightmap,
            WorldBuilder world,
            AnalyzedProject project,
            AnalyzedTypeGraph graph)
        {
            for (int i = 0; i < graph.Positions.Length; ++i)
            {
                var type = project.Types[graph.Ids[i]];
                var position = graph.Positions[i];
                var center = new Point3(position.X, heightmap[position.X, position.Y], position.Y);
                var sentence = Spruce.Generate(
                    seed: type.GetSeed(),
                    size: type.MemberCount);
                Spruce.Draw(
                    sentence: sentence,
                    world: world,
                    position: center,
                    wood: new Block(Colours.Island.Wood),
                    needles: new Block(Colours.Island.Leaves),
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
