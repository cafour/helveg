using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg
{
    public struct World
    {
        private GCHandle chunksHandle;
        private GCHandle positionsHandle;
        private Chunk.Raw[] rawChunks;

        public World(Chunk[] chunks, Vector3[] positions)
        {
            if (chunks.Length != positions.Length)
            {
                throw new ArgumentException("For every chunk there must be a position.");
            }

            Chunks = chunks;
            chunksHandle = default;
            Positions = positions;
            positionsHandle = default;
            rawChunks = Array.Empty<Chunk.Raw>();
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
                Chunks = (Chunk.Raw *)chunksHandle.AddrOfPinnedObject().ToPointer(),
                Positions = (Vector3 *)positionsHandle.AddrOfPinnedObject().ToPointer(),
                ChunkCount = Chunks.Length
            };
        }

        public unsafe struct Raw
        {
            public Chunk.Raw* Chunks;

            public Vector3* Positions;
            public int ChunkCount;
        }
    }
}
