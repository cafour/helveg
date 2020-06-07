using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public class WorldBuilder
    {
        private readonly int chunkSize;
        private readonly Block defaultFill;
        private readonly Vector3[] palette;

        public WorldBuilder(int chunkSize, Block defaultFill, Vector3[] palette)
        {
            this.chunkSize = chunkSize;
            this.defaultFill = defaultFill;
            this.palette = palette;
        }

        public ImmutableDictionary<Vector3, Chunk>.Builder Chunks { get; }
            = ImmutableDictionary.CreateBuilder<Vector3, Chunk>();

        public (int x, int y, int z) GetChunkLocalPosition(Vector3 at)
        {
            var x = ((int)at.X % chunkSize + chunkSize) % chunkSize;
            var y = ((int)at.Y % chunkSize + chunkSize) % chunkSize;
            var z = ((int)at.Z % chunkSize + chunkSize) % chunkSize;
            return (x, y, z);
        }

        public (int x, int y, int z) GetChunkStart(Vector3 at)
        {
            var x = (int)at.X;
            var y = (int)at.Y;
            var z = (int)at.Z;
            x -= x % chunkSize;
            y -= y % chunkSize;
            z -= z % chunkSize;
            return (x, y, z);
        }

        public (Chunk chunk, Vector3 start) GetChunkAt(Vector3 position)
        {
            var (x, y, z) = GetChunkStart(position);
            var at = new Vector3(x, y, z);

            if (Chunks.TryGetValue(at, out var chunk))
            {
                return (chunk, at);
            }

            chunk = Chunk.CreateEmpty(chunkSize, defaultFill, palette);
            Chunks.Add(at, chunk);
            return (chunk, at);
        }

        public void SetBlock(Vector3 at, Block block)
        {
            var (x, y, z) = GetChunkLocalPosition(at);
            var (chunk, _) = GetChunkAt(at);
            chunk.Voxels[x, y, z] = block;
        }

        public Block GetBlock(Vector3 at)
        {
            var (x, y, z) = GetChunkLocalPosition(at);

            var (chunk, _) = GetChunkAt(at);
            return chunk.Voxels[x, y, z];
        }

        public (Vector3 min, Vector3 max) GetBoundingBox()
        {
            var min = new Vector3(
                Chunks.Keys.Min(k => k.X),
                Chunks.Keys.Min(k => k.Y),
                Chunks.Keys.Min(k => k.Z));
            var max = new Vector3(
                Chunks.Keys.Max(k => k.X),
                Chunks.Keys.Max(k => k.Y),
                Chunks.Keys.Max(k => k.Z));
            max += new Vector3(chunkSize, chunkSize, chunkSize);
            return (min, max);
        }

        public void FillColumnTo(Vector2 xz, Block block, int to)
        {
            var chunkHeight = to / chunkSize;
            var (x, _, z) = GetChunkLocalPosition(new Vector3(xz.X, 0, xz.Y));
            for (int i = 0; i < chunkHeight; ++i)
            {
                var (chunk, _) = GetChunkAt(new Vector3(xz.X, i * chunkSize, xz.Y));
                for (int j = 0; j < chunkSize; ++j)
                {
                    if (chunk.Voxels[x, j, z].Flags.HasFlag(BlockFlags.IsAir))
                    {
                        chunk.Voxels[x, j, z] = block;
                    }
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
                    Chunks[key].HollowOut(new Block {Flags = BlockFlags.IsAir});
                }
            }
            return new World(chunkSize, Chunks.ToImmutable());
        }
    }
}
