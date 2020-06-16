using System;
using System.Numerics;
using System.Text;

namespace Helveg.Render
{
    public struct Graph
    {
        public Graph(Vector2[] positions, float[] weights, string[] labels)
        {
            Positions = positions;
            Weights = weights;
            Labels = labels;
        }

        public Vector2[] Positions { get; }

        public float[] Weights { get; }

        public string[] Labels { get; }

        public static float[] UndirectWeights(int[,] directedWeights)
        {
            if (directedWeights.GetLength(0) != directedWeights.GetLength(1))
            {
                throw new ArgumentException("The weights matrix must be square.");
            }

            var nodeCount = directedWeights.GetLength(0);
            var weightCount = nodeCount * (nodeCount - 1) / 2;
            var weights = new float[weightCount];
            int current = 0;
            for (int from = 0; from < nodeCount; ++from)
            {
                for (int to = from + 1; to < nodeCount; ++to, ++current)
                {
                    weights[current] = directedWeights[from, to] + directedWeights[to, from];
                }
            }
            return weights;
        }

        public string ToGraphviz()
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph test {");
            sb.AppendLine("splines=\"line\"");
            sb.AppendLine("scale=72");
            sb.AppendLine("size=36");
            sb.AppendLine("node[shape=circle fixedsize=true width=2]");
            int weightIndex = 0;
            for (int i = 0; i < Positions.Length; ++i, ++weightIndex)
            {
                sb.AppendLine($"{i} [pos=\"{Positions[i].X:0.0000000},{Positions[i].Y:0.0000000}!\" label=\"{Labels[i]}\"]");
                for (int j = i + 1; j < Positions.Length; ++j)
                {
                    if (Weights[weightIndex] != 0)
                    {
                        sb.AppendLine($"{i} -- {j} [weight={Weights[weightIndex]}]");
                    }
                }
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public unsafe struct Raw
        {
            public Vector2* Positions;
            public float* Weights;
            public int Count;
        }
    }
}
