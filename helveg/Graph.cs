using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Helveg
{
    public static class Graph
    {
        public const float RepulsionWeight = 0.1f;
        public const float MinChange = 0.1f;
        public const float Centripetal = 0.1f;
        public const float MaxForcePerStep = 1f;

        public static void ApplyForces(Vector2[] positions, float[,] weights, int maxIterations)
        {
            int nodeCount = positions.Length;
            if (nodeCount != weights.GetLength(0)
                || nodeCount != weights.GetLength(1))
            {
                throw new ArgumentException("The lengths of arguments do not match.");
            }

            Vector2[] forces = new Vector2[nodeCount];
            for (int index = 0; index < maxIterations; ++index)
            {
                for (int from = 0; from < nodeCount; ++from)
                {
                    Vector2 sumForce = Vector2.Zero;
                    for (int to = 0; to < nodeCount; ++to)
                    {
                        if (from == to)
                        {
                            continue;
                        }

                        // https://www.desmos.com/calculator/7ngbnfmcic
                        const float diameter = 2f;
                        const float weightValue = 1f;
                        var direction = positions[to] - positions[from];
                        var length = direction.Length();
                        var unit = direction / length;
                        float weight = weights[from, to] + weights[to, from] > 0 ? weightValue : 0f;
                        // if (weight > 0)
                        // {

                        // }
                        // else
                        // {
                            sumForce += weight * direction;
                            sumForce -= Math.Max(0f, -(length * length) + diameter * diameter + weight * diameter) * unit;
                        // }
                        // // var attraction = 2 * unit * MathF.Log2(length);

                        // if (length < 0)
                        // {
                        //     // radius
                        //     sumForce += unit * length;
                        // }
                        // else if (weight > 0)
                        // {
                        //     sumForce += 2 * unit * MathF.Log2(length);
                        // }
                        // // if (weight > 0)
                        // // {
                        // //     sumForce += ;
                        // // }
                        var centerDistance = positions[from].Length();
                        sumForce += -0.001f * positions[from] / centerDistance * MathF.Log2(centerDistance + 1);
                        // if (weight < 1)
                        // {
                        //     sumForce -= unit / (length * length);
                        // }
                        // else
                        // {
                        //     sumForce += 2 * unit * MathF.Log(length);
                        // }
                    }
                    forces[from] = sumForce * 0.1f;
                }
                for (int i = 0; i < nodeCount; ++i)
                {
                    positions[i] += forces[i];
                }
            }
        }

        public static string Dotify(Vector2[] positions, float[,] weights, string[] labels)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph test {");
            sb.AppendLine("splines=\"line\"");
            sb.AppendLine("scale=72");
            sb.AppendLine("size=36");
            sb.AppendLine("node[shape=circle fixedsize=true width=2]");
            for (int i = 0; i < positions.Length; ++i)
            {
                sb.AppendLine($"{i} [pos=\"{positions[i].X},{positions[i].Y}!\" label=\"{labels[i]}\"]");
                for (int j = 0; j < positions.Length; ++j)
                {
                    if (weights[i, j] != 0) {
                        sb.AppendLine($"{i} -> {j} [weight={(int)MathF.Round(weights[i, j])}]");
                    }
                }
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
