using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg.Render
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Label
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Text;
        public Vector3 Position;
        public Vector2 Size;
    }
}
