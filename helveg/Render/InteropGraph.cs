using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg.Render
{
    public struct InteropGraph
    {
        public InteropGraph(Vector2[] positions, float[] weights)
        {
            Positions = positions;
            Weights = weights;
        }

        public Vector2[] Positions { get; }

        public float[] Weights { get; }

        public unsafe struct Raw
        {
            public Vector2 *Positions;
            public float *Weights;
            public int Count;
        }
    }
}
