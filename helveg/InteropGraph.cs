using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg
{
    public struct InteropGraph
    {
        public InteropGraph(Vector2[] positions, int[] weights)
        {
            Positions = positions;
            Weights = weights;
        }

        public Vector2[] Positions { get; }

        public int[] Weights { get; }

        public unsafe struct Raw
        {
            public Vector2 *Positions;
            public int *Weights;
            public int Count;
        }
    }
}
