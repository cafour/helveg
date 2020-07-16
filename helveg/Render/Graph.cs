using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Helveg.Analysis;

namespace Helveg.Render
{
    public struct Graph
    {
        public Graph(string name, Vector2[] positions, float[] weights, string[] labels, float[] sizes)
        {
            Name = name;
            Positions = positions;
            Weights = weights;
            Labels = labels;
            Sizes = sizes;
        }

        public string Name { get; }

        public Vector2[] Positions { get; }

        public float[] Weights { get; }

        public string[] Labels { get; }

        public float[] Sizes { get; }

        public static Graph FromAnalyzed(AnalyzedTypeGraph typeGraph)
        {
            return new Graph(
                name: typeGraph.ProjectName,
                positions: typeGraph.Positions,
                weights: UndirectWeights(typeGraph.Weights),
                labels: typeGraph.Ids.Select(i => i.ToString()).ToArray(),
                sizes: typeGraph.Sizes.Select(s => (float)s).ToArray()
            );
        }

        public static Graph FromAnalyzed(AnalyzedProject project)
        {
            return FromAnalyzed(Analyze.GetTypeGraph(project));
        }

        public static Graph FromAnalyzed(AnalyzedSolution solution)
        {
            var names = solution.Projects.Keys.OrderBy(k => k).ToArray();
            var weights = new float[names.Length * (names.Length - 1)];
            var weightIndex = 0;
            for (int from = 0; from < names.Length; ++from)
            {
                for (int to = from + 1; to < names.Length; ++to, ++weightIndex)
                {
                    weights[weightIndex] = solution.Projects[names[from]].ProjectReferences.Contains(names[to])
                        || solution.Projects[names[to]].ProjectReferences.Contains(names[from])
                        ? 1
                        : 0;
                }
            }

            var sizes = names.Select(n => (float)solution.Projects[n].Types.Count).ToArray();
            return new Graph(solution.Name, new Vector2[solution.Projects.Count], weights, names, sizes);
        }

        public AnalyzedTypeGraph ToAnalyzed()
        {
            var graph = new AnalyzedTypeGraph
            {
                Ids = Labels.Select(l => AnalyzedTypeId.Parse(l)).ToArray(),
                Positions = Positions,
                Sizes = Sizes.Select(s => (int)MathF.Round(s)).ToArray(),
                ProjectName = Name,
                Weights = new int[Positions.Length, Positions.Length]
            };

            int weightIndex = 0;
            for (int from = 0; from < Positions.Length; ++from)
            {
                for (int to = from + 1; to < Positions.Length; ++to, ++weightIndex)
                {
                    var weight = (int)MathF.Round(Weights[weightIndex]);
                    graph.Weights[from, to] = weight;
                    graph.Weights[to, from] = weight;
                }
            }
            return graph;
        }

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

        public RectangleF GetBoundingBox()
        {
            var max = new Vector2(float.MinValue, float.MinValue);
            var min = new Vector2(float.MaxValue, float.MaxValue);
            for (int i = 0; i < Positions.Length; ++i)
            {
                max = new Vector2(
                    x: MathF.Max(max.X, Positions[i].X + Sizes[i]),
                    y: MathF.Max(max.Y, Positions[i].Y + Sizes[i]));
                min = new Vector2(
                    x: MathF.Min(min.X, Positions[i].X - Sizes[i]),
                    y: MathF.Min(min.Y, Positions[i].Y - Sizes[i]));
            }
            return new RectangleF(min.X, min.Y, MathF.Abs(max.X - min.X), MathF.Abs(max.Y - min.Y));
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
