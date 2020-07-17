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
                        sum = MathF.Max(sum, GrassLevel * (1.0f - Glsl.Smoothstep(0.0f, SlopeWidth, l - r)));
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
                for (int x = heightmap.MinX; x < heightmap.MaxX; ++x)
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

        public static void PlaceCargo(
            WorldBuilder world,
            AnalyzedProject project,
            RectangleF boundingBox)
        {
            const int offset = 20;
            var random = new Random(project.GetSeed());
            var dockside = new Vector2[]
            {
                new Vector2(1, 0),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(0, -1)
            }[random.Next() % 4];
            var center = new Vector2(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
            var heightmapPosition = center
                + new Vector2(boundingBox.Width / 2 + offset, boundingBox.Height / 2 + offset) * dockside;
            var position = new Point3(heightmapPosition.X, WaterLevel + 1, heightmapPosition.Y);
            Cargo.Draw(
                world: world,
                position: position,
                platform: new Block(Colours.Island.Wood),
                corner: new Block(Colours.Island.Stone),
                containers: new[]
                {
                    new Block(Colours.Island.Cargo0),
                    new Block(Colours.Island.Cargo1),
                    new Block(Colours.Island.Cargo2)
                },
                seed: project.GetSeed(),
                containerCount: project.PackageReferences.Count);
        }

        public static void PlaceTypeStructures(
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
                switch (type.Kind)
                {
                    case AnalyzedTypeKind.Class:
                        var sentence = Spruce.Generate(
                            seed: type.GetSeed(),
                            size: type.MemberCount);
                        // the byte cast being required twice is beyond me
                        var needles = (byte)((byte)Colours.Island.Needles0
                            + (byte)type.Family % Colours.NeedleColourCount);
                        Spruce.Draw(
                            sentence: sentence,
                            world: world,
                            position: center,
                            wood: new Block(Colours.Island.Wood),
                            needles: new Block { PaletteIndex = needles },
                            hasNeedles: !type.Health.HasFlag(Diagnosis.Warning));
                        break;
                    case AnalyzedTypeKind.Struct:
                        LogCabin.Draw(
                            world: world,
                            position: center,
                            wood0: new Block(Colours.Island.Wood0),
                            wood1: new Block(Colours.Island.Wood1),
                            roof: new Block(Colours.Island.Roof),
                            side: (int)MathF.Round(MathF.Sqrt(type.MemberCount)) + 1,
                            levelCount: type.MemberCount,
                            hasRoof: !type.Health.HasFlag(Diagnosis.Warning));
                        break;
                }
                if (type.Health.HasFlag(Diagnosis.Error))
                {
                    world.Burn(center + new Point3(0, 6, 0), MathF.Log2(type.MemberCount) * 2);
                }
            }
        }

        public static bool PlaceBridges(
            Heightmap heightmap,
            WorldBuilder world,
            Graph solutionGraph,
            AnalyzedProject project)
        {
            const float edgeInset = 10.0f;
            var index = Array.IndexOf(solutionGraph.Labels, project.Name);
            if (index == -1)
            {
                return false;
            }

            var center = solutionGraph.Positions[index];
            foreach (var dependency in project.ProjectReferences)
            {
                var dependencyIndex = Array.IndexOf(solutionGraph.Labels, dependency);
                if (dependencyIndex == -1)
                {
                    return false;
                }

                var dependencyCenter = solutionGraph.Positions[dependencyIndex];
                var direction = Vector2.Normalize(dependencyCenter - center);
                var from = FindIslandEdge(heightmap, center, direction) - edgeInset * direction;
                var to = FindIslandEdge(heightmap, dependencyCenter, -direction) + edgeInset * direction;
                Bridge.Draw(
                    world: world,
                    from: new Point3(from.X, heightmap[from], from.Y),
                    to: new Point3(to.X, heightmap[to], to.Y),
                    bridge: new Block[] {new Block(Colours.Island.Cargo0), new Block(Colours.Island.Cargo1)},
                    height: (int)MathF.Sqrt((dependencyCenter - center).Length()));
            }
            return true;
        }

        public static Vector2 FindIslandEdge(Heightmap heightmap, Vector2 from, Vector2 direction)
        {
            var current = from;
            while(heightmap[current] > WaterLevel)
            {
                current += direction;
            }
            return current - direction;
        }
    }
}
