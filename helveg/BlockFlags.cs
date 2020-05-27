using System;

namespace Helveg
{
    [Flags]
    public enum BlockFlags : byte
    {
        IsAir = 1 << 0,
    }
}
