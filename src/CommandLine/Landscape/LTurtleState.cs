using System.Collections.Immutable;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public struct LTurtleState
    {
        public Point3 Position;
        public Quaternion Orientation;
        public Vector3 Forward;
        public ImmutableArray<float> Parameters;
    }
}
