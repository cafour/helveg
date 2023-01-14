using System;

namespace Helveg.Render
{
    [Flags]
    public enum BlockFlags : byte
    {
        None = 0,
        IsAir = 1 << 0,
    }
}
