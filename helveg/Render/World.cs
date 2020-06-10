using System;
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

        public World(Chunk[] chunks, Point3[] positions)
            : this(chunks, positions.Select(p => (Vector3)p).ToArray())
        {
        }


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

        public Chunk[] Chunks;

        public Vector3[] Positions;

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

        public unsafe struct Raw
        {
            public Chunk.Raw* Chunks;
            public Vector3* Positions;
            public uint ChunkCount;
        }
    }
}
