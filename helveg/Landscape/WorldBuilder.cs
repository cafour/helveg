using System;
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

        public ImmutableDictionary<Point3, Chunk>.Builder Chunks { get; }
            = ImmutableDictionary.CreateBuilder<Point3, Chunk>();

        public int ChunkSize { get; }

        public Chunk GetChunkAt(Point3 position)
        {
            var chunkStart = position - position % ChunkSize;

            if (Chunks.TryGetValue(chunkStart, out var chunk))
            {
                return chunk;
            }

            chunk = Chunk.CreateEmpty(ChunkSize, defaultFill, palette);
            Chunks.Add(chunkStart, chunk);
            return chunk;
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

        public void FillLine(Point3 from, Point3 to, Block fill)
        {
            // just bresenham in 3d
            var diff = to - from;
            var sign = Point3.Sign(diff);
            var max = Point3.Max(diff);
            var length = Math.Abs(max);
            var error = 2 * diff - new Point3(max);

            this[from] = fill;
            if (max == diff.X)
            {
                for (int dx = 0; dx < length; ++dx)
                {
                    from.X += sign.X;
                    if (error.Y >= 0)
                    {
                        from.Y += sign.Y;
                        error.Y -= 2 * diff.X;
                    }

                    if (error.Z >= 0)
                    {
                        from.Z += sign.Z;
                        error.Z -= 2 * diff.X;
                    }

                    error.Y += 2 * diff.Y;
                    error.Z += 2 * diff.Z;
                    this[from] = fill;
                }
            }
            else if(max == diff.Y)
            {
                for (int dy = 0; dy < length; ++dy)
                {
                    from.Y += sign.Y;
                    if (error.X >= 0)
                    {
                        from.X += sign.X;
                        error.X -= 2 * diff.Y;
                    }

                    if (error.Z >= 0)
                    {
                        from.Z += sign.Z;
                        error.Z -= 2 * diff.X;
                    }

                    error.X += 2 * diff.X;
                    error.Z += 2 * diff.Z;
                    this[from] = fill;
                }
            }
            else
            {
                for (int dz = 0; dz < length; ++dz)
                {
                    from.Z += sign.Z;
                    if (error.Y >= 0)
                    {
                        from.Y += sign.Y;
                        error.Y -= 2 * diff.X;
                    }

                    if (error.X >= 0)
                    {
                        from.X += sign.X;
                        error.X -= 2 * diff.Y;
                    }

                    error.Y += 2 * diff.Y;
                    error.X += 2 * diff.X;
                    this[from] = fill;
                }
            }
        }

        public World Build()
        {
            var keys = Chunks.Keys.ToImmutableArray();
            foreach (var key in keys)
            {
                if (Chunks[key].IsAir())
                {
                    Chunks.Remove(key);
                }
                else
                {
                    Chunks[key].HollowOut(new Block { Flags = BlockFlags.IsAir });
                }
            }
            return new World(Chunks.Values.ToArray(), Chunks.Keys.ToArray());
        }
    }
}
