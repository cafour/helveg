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
        private GCHandle firesHandle;
        private Chunk.Raw[] rawChunks;

        public World(Chunk[] chunks, Point3[] positions, Emitter[] fires)
            : this(chunks, positions.Select(p => (Vector3)p).ToArray(), fires)
        {
        }

        public World(Chunk[] chunks, Vector3[] positions, Emitter[] fires)
        {
            if (chunks.Length != positions.Length)
            {
                throw new ArgumentException("For every chunk there must be a position.");
            }

            Chunks = chunks;
            chunksHandle = default;
            Positions = positions;
            positionsHandle = default;
            Fires = fires;
            firesHandle = default;
            rawChunks = Array.Empty<Chunk.Raw>();
        }

        public Chunk[] Chunks;

        public Vector3[] Positions;

        public Emitter[] Fires;

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

            if (!firesHandle.IsAllocated)
            {
                firesHandle = GCHandle.Alloc(Fires, GCHandleType.Pinned);
            }

            return new Raw
            {
                Chunks = (Chunk.Raw*)chunksHandle.AddrOfPinnedObject().ToPointer(),
                ChunkOffsets = (Vector3*)positionsHandle.AddrOfPinnedObject().ToPointer(),
                ChunkCount = (uint)Chunks.Length,
                Fires = (Emitter*)firesHandle.AddrOfPinnedObject().ToPointer(),
                FireCount = (uint)Fires.Length
            };
        }

        public unsafe struct Raw
        {
            public Chunk.Raw* Chunks;
            public Vector3* ChunkOffsets;
            public uint ChunkCount;
            public Emitter* Fires;
            public uint FireCount;
        }
    }
}
