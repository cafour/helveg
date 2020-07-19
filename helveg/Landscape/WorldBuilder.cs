using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public class WorldBuilder
    {
        private readonly Block defaultFill;
        private readonly Vector3[] palette;

        public WorldBuilder(int chunkSize, Block defaultFill, Vector3[] palette)
        {
            ChunkSize = chunkSize;
            this.defaultFill = defaultFill;
            this.palette = palette;
        }

        public ConcurrentDictionary<Point3, Chunk> Chunks { get; } = new ConcurrentDictionary<Point3, Chunk>();

        public List<Emitter> Fires { get; } = new List<Emitter>();

        public int ChunkSize { get; }

        public Vector3 SkyColour { get; set; } = Colours.SkyColour;

        public Vector3 InitialCameraPosition { get; set; }

        public Chunk GetChunkAt(Point3 position)
        {
            var chunkStart = position - ((position % ChunkSize) + new Point3(ChunkSize)) % ChunkSize;

            Chunk chunk;
            while (!Chunks.TryGetValue(chunkStart, out chunk))
            {
                chunk = Chunk.CreateEmpty(ChunkSize, defaultFill, palette);
                Chunks.TryAdd(chunkStart, chunk);
            }
            return chunk;
        }

        public int GetHeight(int x, int z)
        {
            int y = 0;
            while (!this[x, y, z].Flags.HasFlag(BlockFlags.IsAir))
            {
                y++;
            }
            return y - 1;
        }

        public Block this[Point3 at]
        {
            get
            {
                var chunk = GetChunkAt(at);
                var local = (at % ChunkSize + new Point3(ChunkSize)) % ChunkSize;
                return chunk.Voxels[local.X, local.Y, local.Z];
            }
            set
            {
                var chunk = GetChunkAt(at);
                var local = (at % ChunkSize + new Point3(ChunkSize)) % ChunkSize;
                chunk.Voxels[local.X, local.Y, local.Z] = value;
            }
        }

        public Block this[int x, int y, int z]
        {
            get => this[new Point3(x, y, z)];
            set => this[new Point3(x, y, z)] = value;
        }

        public (Point3 min, Point3 max) GetBoundingBox()
        {
            var min = new Point3(
                Chunks.Keys.Min(k => k.X),
                Chunks.Keys.Min(k => k.Y),
                Chunks.Keys.Min(k => k.Z));
            var max = new Point3(
                Chunks.Keys.Max(k => k.X) + ChunkSize,
                Chunks.Keys.Max(k => k.Y) + ChunkSize,
                Chunks.Keys.Max(k => k.Z) + ChunkSize);
            return (min, max);
        }

        public void OverLine(Point3 from, Point3 to, Action<Point3> action, bool corners = false)
        {
            // just bresenham in 3d
            var diff = to - from;
            var sign = Point3.Sign(diff);
            diff = Point3.Abs(diff);
            var max = Point3.Max(diff);

            var error = 2 * diff - new Point3(max);
            action(from);

            void actionCorner(Point3 p)
            {
                if (corners)
                {
                    action(p);
                }
            };

            if (max == diff.X)
            {
                for (int dx = 0; dx < max; ++dx)
                {
                    from.X += sign.X;
                    if (error.Y >= 0)
                    {
                        actionCorner(from);
                        from.Y += sign.Y;
                        error.Y -= 2 * diff.X;
                    }

                    if (error.Z >= 0)
                    {
                        actionCorner(from);
                        from.Z += sign.Z;
                        error.Z -= 2 * diff.X;
                    }

                    error.Y += 2 * diff.Y;
                    error.Z += 2 * diff.Z;
                    action(from);
                }
            }
            else if (max == diff.Y)
            {
                for (int dy = 0; dy < max; ++dy)
                {
                    from.Y += sign.Y;
                    if (error.X >= 0)
                    {
                        actionCorner(from);
                        from.X += sign.X;
                        error.X -= 2 * diff.Y;
                    }

                    if (error.Z >= 0)
                    {
                        actionCorner(from);
                        from.Z += sign.Z;
                        error.Z -= 2 * diff.Y;
                    }

                    error.X += 2 * diff.X;
                    error.Z += 2 * diff.Z;
                    action(from);
                }
            }
            else
            {
                for (int dz = 0; dz < max; ++dz)
                {
                    from.Z += sign.Z;
                    if (error.Y >= 0)
                    {
                        actionCorner(from);
                        from.Y += sign.Y;
                        error.Y -= 2 * diff.Z;
                    }

                    if (error.X >= 0)
                    {
                        actionCorner(from);
                        from.X += sign.X;
                        error.X -= 2 * diff.Z;
                    }

                    error.Y += 2 * diff.Y;
                    error.X += 2 * diff.X;
                    action(from);
                }
            }
        }

        public void FillLine(Point3 from, Point3 to, Block fill, bool corners = false)
        {
            OverLine(from, to, p => this[p] = fill, corners);
        }

        public void FillSphere(Point3 position, Block fill, int radius)
        {
            for (int x = 0; x < radius; ++x)
            {
                for (int y = 0; y < radius; ++y)
                {
                    for (int z = 0; z < radius; ++z)
                    {
                        if (x * x + y * y + z * z < radius * radius)
                        {
                            this[new Point3(x, y, z) + position] = fill;
                            this[new Point3(x, y, -z) + position] = fill;
                            this[new Point3(x, -y, z) + position] = fill;
                            this[new Point3(x, -y, -z) + position] = fill;
                            this[new Point3(-x, y, z) + position] = fill;
                            this[new Point3(-x, y, -z) + position] = fill;
                            this[new Point3(-x, -y, z) + position] = fill;
                            this[new Point3(-x, -y, -z) + position] = fill;
                        }
                    }
                }
            }
        }

        public void FillCube(Point3 position, Block fill, int radius)
        {
            for (int x = -radius; x <= radius; ++x)
            {
                for (int y = -radius; y <= radius; ++y)
                {
                    for (int z = -radius; z <= radius; ++z)
                    {
                        this[position + new Point3(x, y, z)] = fill;
                    }
                }
            }
        }

        public void FillVolume(Point3 one, Point3 two, Block fill)
        {
            var min = new Point3(Math.Min(one.X, two.X), Math.Min(one.Y, two.Y), Math.Min(one.Z, two.Z));
            var max = new Point3(Math.Max(one.X, two.X), Math.Max(one.Y, two.Y), Math.Max(one.Z, two.Z));
            for (int x = min.X; x <= max.X; ++x)
            {
                for (int y = min.Y; y <= max.Y; ++y)
                {
                    for (int z = min.Z; z <= max.Z; ++z)
                    {
                        this[x, y, z] = fill;
                    }
                }
            }
        }

        public void FillPipe(Point3 from, Point3 to, Block fill, int radius)
        {
            OverLine(from, to, p => FillCube(p, fill, radius));
        }

        public void Burn(Point3 what, float radius)
        {
            Fires.Add(new Emitter { Position = (Vector3)what, Radius = radius });
        }

        public World Build()
        {
            var keys = Chunks.Keys.ToImmutableArray();
            foreach (var key in keys)
            {
                if (Chunks[key].IsAir())
                {
                    Chunks.TryRemove(key, out _);
                }
            }
            return new World(
                Chunks.Values.ToArray(),
                Chunks.Keys.ToArray(),
                Fires.ToArray(),
                SkyColour,
                InitialCameraPosition);
        }
    }
}
