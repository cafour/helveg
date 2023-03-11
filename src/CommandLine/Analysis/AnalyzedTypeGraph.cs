using System.Numerics;

namespace Helveg.Analysis
{
    /// <summary>
    /// A transition between an AnalyzedProject and a Graph.
    /// <summary>
    public struct AnalyzedTypeGraph
    {
        public string ProjectName;
        public AnalyzedTypeId[] Ids;
        public Vector2[] Positions;
        public int[] Sizes;
        public int[,] Weights;
    }
}
