using System;

namespace Helveg
{
    public struct Chunk
    {
        public Chunk(BlockKind[,,] blocks)
        {
            if (blocks.GetLength(0) != blocks.GetLength(1)
                || blocks.GetLength(1) != blocks.GetLength(2))
            {
                throw new ArgumentException("The block array must be a cube.");
            }
            Blocks = blocks;
        }

        public BlockKind[,,] Blocks { get; }

        public unsafe struct Raw
        {
            public BlockKind* Blocks;
            public int Size;
        }
    }
}
