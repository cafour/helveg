using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Helveg.Render
{
    [DebuggerDisplay("[{PaletteIndex}:{Flags}]")]
    public struct Block : IEquatable<Block>
    {
        public byte PaletteIndex;
        public BlockFlags Flags;

        public Block(Enum paletteValue)
        {
            Flags = BlockFlags.None;
            PaletteIndex = Convert.ToByte(paletteValue);
        }

        public static bool operator ==(Block left, Block right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Block left, Block right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Block block)
            {
                return Equals(block);
            }

            return false;
        }

        public bool Equals([AllowNull] Block other)
        {
            return PaletteIndex == other.PaletteIndex
                && Flags == other.Flags;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PaletteIndex, Flags);
        }
    }
}
