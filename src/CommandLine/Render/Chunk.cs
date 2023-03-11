using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Helveg.Landscape;

namespace Helveg.Render
{
    public struct Chunk
    {
        private GCHandle voxelsHandle;
        private GCHandle paletteHandle;

        public Chunk(Block[,,] voxels, Vector3[] palette)
        {
            if (voxels.GetLength(0) != voxels.GetLength(1)
                || voxels.GetLength(1) != voxels.GetLength(2))
            {
                throw new ArgumentException("The colors array must be a cube.");
            }
            Voxels = voxels;
            voxelsHandle = default;
            Palette = palette;
            paletteHandle = default;
        }

        public Block[,,] Voxels;
        public Vector3[] Palette;

        public static Chunk CreateEmpty(int size, Block fill, Vector3[] palette)
        {
            var voxels = new Block[size, size, size];
            // TODO: this is horrible, please find a way to initialize it directly
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int z = 0; z < size; ++z)
                    {
                        voxels[x, y, z] = fill;
                    }
                }
            }
            return new Chunk(voxels, palette);
        }

        public static Chunk CreateHorizontalPlane(
            int size,
            Vector3[] palette,
            Block planeBlock,
            int y = 0)
        {
            var chunk = CreateEmpty(size, new Block { Flags = BlockFlags.IsAir }, palette);
            for (int x = 0; x < size; ++x)
            {
                for (int z = 0; z < size; ++z)
                {
                    chunk.Voxels[x, y, z] = planeBlock;
                }
            }
            return chunk;
        }

        public static Chunk CreateNoisy(
            int size,
            Vector3[] palette,
            Block fill,
            long seed,
            double frequency,
            Vector2 offset)
        {
            var air = new Block { Flags = BlockFlags.IsAir };
            var terrain = new double[size, size];
            var noise = OpenSimplex.Create(seed);
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    var noiseValue = noise.Evaluate(frequency * (x + offset.X), frequency * (y + offset.Y));
                    terrain[x, y] = (noiseValue + 1.0) / 2.0 * size;
                }
            }

            var voxels = new Block[size, size, size];
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int z = 0; z < size; ++z)
                    {
                        voxels[x, y, z] = y > terrain[x, z] ? air : fill;
                    }
                }
            }
            var chunk = new Chunk(voxels, palette);
            return chunk;
        }

        public void HollowOut(Block air)
        {
            var size = Voxels.GetLength(0);
            var hollowed = (Block[,,])Voxels.Clone();
            Array.Copy(Voxels, hollowed, Voxels.Length);
            for (int x = 1; x < size - 1; ++x)
            {
                for (int y = 1; y < size - 1; ++y)
                {
                    for (int z = 1; z < size - 1; ++z)
                    {
                        var airAdjacent = Voxels[x - 1, y, z] == air
                            || Voxels[x + 1, y, z] == air
                            || Voxels[x, y - 1, z] == air
                            || Voxels[x, y + 1, z] == air
                            || Voxels[x, y, z - 1] == air
                            || Voxels[x, y, z + 1] == air;
                        hollowed[x, y, z] = airAdjacent ? Voxels[x, y, z] : air;
                    }
                }
            }
            Array.Copy(hollowed, Voxels, hollowed.Length);
        }

        public bool IsAir()
        {
            for (int x = 0; x < Voxels.GetLength(0); ++x)
            {
                for (int y = 0; y < Voxels.GetLength(1); ++y)
                {
                    for (int z = 0; z < Voxels.GetLength(2); ++z)
                    {
                        if (!Voxels[x, y, z].Flags.HasFlag(BlockFlags.IsAir))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public unsafe Raw GetRaw()
        {
            if (!voxelsHandle.IsAllocated)
            {
                voxelsHandle = GCHandle.Alloc(Voxels, GCHandleType.Pinned);
            }

            if (!paletteHandle.IsAllocated)
            {
                paletteHandle = GCHandle.Alloc(Palette, GCHandleType.Pinned);
            }

            return new Raw
            {
                Voxels = (Block*)voxelsHandle.AddrOfPinnedObject().ToPointer(),
                Palette = (Vector3*)paletteHandle.AddrOfPinnedObject().ToPointer(),
                Size = Voxels.GetLength(0)
            };
        }

        public unsafe struct Raw
        {
            public Block* Voxels;
            public Vector3* Palette;
            public int Size;
        }
    }
}
