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

        public string ToGraphviz(int scale = 72, int size = 36, int nodeWidth = 2)
        {
            var sb = new StringBuilder();
            sb.AppendLine("graph test {");
            sb.AppendLine("splines=\"line\"");
            sb.AppendLine($"scale={scale}");
            sb.AppendLine($"size={size}");
            sb.AppendLine($"node[shape=circle fixedsize=true width={nodeWidth}]");
            int weightIndex = 0;
            for (int from = 0; from < Positions.Length; ++from)
            {
                sb.AppendLine($"{from} [pos=\"{Positions[from].X:0.0000000},{Positions[from].Y:0.0000000}!\" label=\"{Labels[from]}\"]");
                for (int to = from + 1; to < Positions.Length; ++to, ++weightIndex)
                {
                    if (Weights[weightIndex] != 0)
                    {
                        sb.AppendLine($"{from} -- {to} [weight={Weights[weightIndex]}]");
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
