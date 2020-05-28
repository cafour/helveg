using System.Diagnostics;

namespace Helveg
{
    [DebuggerDisplay("[{PaletteIndex}:{Flags}]")]
    public struct Block
    {
        public byte PaletteIndex;
        public BlockFlags Flags;
    }
}
