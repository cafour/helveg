using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg.Render
{
    public struct World
    {
        private GCHandle chunksHandle;
        private GCHandle positionsHandle;
        private Chunk.Raw[] rawChunks;
        private readonly ImmutableDictionary<Vector3, Chunk> chunkMap;
        private readonly int chunkSize;

        public World(int chunkSize, Chunk[] chunks, Vector3[] positions)
        {
            if (chunks.Length != positions.Length)
            {
                throw new ArgumentException("For every chunk there must be a position.");
            }

            this.chunkSize = chunkSize;
            Chunks = chunks;
            chunksHandle = default;
            Positions = positions;
            positionsHandle = default;
            rawChunks = Array.Empty<Chunk.Raw>();

            var chunkIndicesBuilder = ImmutableDictionary.CreateBuilder<Vector3, Chunk>();
            for (int i = 0; i < positions.Length; ++i)
            {
                chunkIndicesBuilder.Add(positions[i], chunks[i]);
            }
            chunkMap = chunkIndicesBuilder.ToImmutable();
        }

        public World(int chunkSize, ImmutableDictionary<Vector3, Chunk> chunkMap)
        {
            this.chunkSize = chunkSize;
            Chunks = chunkMap.Values.ToArray();
            chunksHandle = default;
            Positions = chunkMap.Keys.ToArray();
            positionsHandle = default;
            rawChunks = Array.Empty<Chunk.Raw>();
            this.chunkMap = chunkMap;
        }

        public Chunk[] Chunks { get; }

        public Vector3[] Positions { get; }

        public unsafe Raw GetRaw()
        {
            if (!chunksHandle.IsAllocated)
            {
                rawChunks = new Chunk.Raw[Chunks.Length];
                for (int i = 0; i < Chunks.Length; ++i)
                {
                    rawChunks[i] = Chunks[i].GetRaw();
                }
                chunksHandle = GCHandle.Alloc(rawChunks, GCHandleType.Pinned);
            }

            if (!positionsHandle.IsAllocated)
            {
                positionsHandle = GCHandle.Alloc(Positions, GCHandleType.Pinned);
            }

            return new Raw
            {
                Chunks = (Chunk.Raw*)chunksHandle.AddrOfPinnedObject().ToPointer(),
                Positions = (Vector3*)positionsHandle.AddrOfPinnedObject().ToPointer(),
                ChunkCount = (uint)Chunks.Length
            };
        }

        public bool TryGetChunkAt(Vector3 position, out Chunk chunk)
        {
            var chunkStart = new Vector3(
                x: position.X - position.X % chunkSize,
                y: position.Y - position.Y % chunkSize,
                z: position.Z - position.Z % chunkSize);
            return chunkMap.TryGetValue(chunkStart, out chunk);
        }

        public unsafe struct Raw
        {
            public Chunk.Raw* Chunks;

            public Vector3* Positions;
            public uint ChunkCount;
        }
    }
}
