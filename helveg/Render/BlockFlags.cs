using System;

namespace Helveg.Render
{
    [Flags]
    public enum BlockFlags : byte
    {
        IsAir = 1 << 0,
    }
}
