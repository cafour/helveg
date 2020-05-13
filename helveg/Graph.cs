using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Helveg
{
    public static class Graph
    {

        public static void StepEades(Vector2[] forces, Vector2[] positions, float[,] weights)
        {
            int nodeCount = positions.Length;
            for (int from = 0; from < nodeCount; ++from)
            {
                var centerDistance = positions[from].Length();
                forces[from] = -0.5f * positions[from] / centerDistance * MathF.Log2(centerDistance + 1);
                for (int to = from + 1; to < nodeCount; ++to)
                {
                    var direction = positions[to] - positions[from];
                    var length = direction.Length();
                    var unit = direction / length;
                    var force = weights[from, to] + weights[to, from] > 0f
                        ? 2f * MathF.Log2(length)
                        : -1f / (length * length);
                    force = Math.Clamp(force, -nodeCount, nodeCount);
                    forces[from] += force * unit;
                    forces[to] -= force * unit;
                }
            }
            for (int i = 0; i < nodeCount; ++i)
            {
                positions[i] += 0.1f * forces[i];
            }
        }

        public static void RunEades(Vector2[] positions, float[,] weights, int iterationCount)
        {
            int nodeCount = positions.Length;
            if (nodeCount != weights.GetLength(0)
                || nodeCount != weights.GetLength(1))
            {
                throw new ArgumentException("The lengths of arguments do not match.");
            }

            Vector2[] forces = new Vector2[nodeCount];
            for (int index = 0; index < iterationCount; ++index)
            {
                StepEades(forces, positions, weights);
            }
        }

        public static void StepFruchtermanReingold(
            Vector2[] displacement,
            float width,
            float height,
            float temperature,
            float k,
            Vector2[] positions,
            float[,] weights)
        {
            int nodeCount = positions.Length;

            // calculate repulsive forces
            for (int from = 0; from < nodeCount; ++from)
            {
                displacement[from] = Vector2.Zero;
                for (int to = 0; to < nodeCount; ++to)
                {
                    if (from == to)
                    {
                        continue;
                    }

                    var direction = positions[from] - positions[to];
                    var length = direction.Length();
                    if (length > 0)
                    {
                        var unit = direction / length;
                        var force = k * k / length;
                        displacement[from] += force * unit;
                    }
                }
            }

            // calculate attractive forces
            for (int from = 0; from < nodeCount; ++from)
            {
                for (int to = from + 1; to < nodeCount; ++to)
                {
                    if (weights[from, to] + weights[to, from] > 0)
                    {
                        var direction = positions[from] - positions[to];
                        var length = direction.Length();
                        if (length > 0)
                        {
                            var unit = direction / length;
                            var force = length * length / k;
                            displacement[from] -= unit * force;
                            displacement[to] += unit * force;
                        }
                    }
                }
            }

            // limit max displacement to temperature 
            for (int i = 0; i < nodeCount; ++i)
            {
                var length = displacement[i].Length();
                positions[i] += displacement[i] / length * MathF.Min(length, temperature);
                positions[i].X = MathF.Min(width / 2f, MathF.Max(-width / 2f, positions[i].X));
                positions[i].Y = MathF.Min(height / 2f, MathF.Max(-height / 2f, positions[i].Y));
            }
        }

        public static void RunFruchtermanReingold(Vector2[] positions, float[,] weights, int iterationCount)
        {
            int nodeCount = positions.Length;
            if (nodeCount != weights.GetLength(0)
                || nodeCount != weights.GetLength(1))
            {
                throw new ArgumentException("The lengths of arguments do not match.");
            }

            float width = 64f;
            float height = 64f;
            var area = width * height;
            var k = MathF.Sqrt(area / nodeCount) / 3f;
            var maxTemperature = width / 10f;
            var temperature = maxTemperature;

            Vector2[] displacement = new Vector2[nodeCount];
            for (int index = 0; index < iterationCount; ++index)
            {
                StepFruchtermanReingold(displacement, width, height, temperature, k, positions, weights);
                temperature = maxTemperature / (index + 2);
            }
        }


        public static string ToGraphviz(Vector2[] positions, float[,] weights, string[] labels)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph test {");
            sb.AppendLine("splines=\"line\"");
            sb.AppendLine("scale=72");
            sb.AppendLine("size=36");
            sb.AppendLine("node[shape=circle fixedsize=true width=2]");
            for (int i = 0; i < positions.Length; ++i)
            {
                sb.AppendLine($"{i} [pos=\"{positions[i].X:0.0000000},{positions[i].Y:0.0000000}!\" label=\"{labels[i]}\"]");
                for (int j = 0; j < positions.Length; ++j)
                {
                    if (weights[i, j] != 0)
                    {
                        sb.AppendLine($"{i} -> {j} [weight={(int)MathF.Round(weights[i, j])}]");
                    }
                }
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
