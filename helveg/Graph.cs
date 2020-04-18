using System;
using System.Numerics;
using System.Text;

namespace Helveg
{
    public static class Graph
    {
        public const float RepulsionWeight = 3.6f;
        public const float MinChange = 0.1f;

        public static Vector2[] ApplyForces(Vector2[] positions, float[,] weights, int maxIterations)
        {
            if (positions.Length != weights.GetLength(0)
                || positions.Length != weights.GetLength(1))
            {
                throw new ArgumentException("The lengths of arguments do not match.");
            }

            Vector2[] buffer = new Vector2[positions.Length];
            positions.CopyTo(buffer, 0);
            bool hasChanged = true;

            int nodeCount = positions.Length;
            int index = 0;
            for (; index < maxIterations && hasChanged; ++index)
            {
                hasChanged = false;
                Vector2[] input = (index % 2) == 1 ? buffer : positions;
                Vector2[] result = (index % 2) == 0 ? buffer : positions;
                for (int from = 0; from < nodeCount; ++from)
                {
                    Vector2 sumForce = Vector2.Zero;
                    for (int to = 0; to < nodeCount; ++to)
                    {
                        float weight = weights[from, to];
                        if (weight == 0)
                        {
                            continue;
                        }

                        var direction = input[to] - input[from];
                        var unit = direction / direction.Length();
                        sumForce += unit * weight / direction.LengthSquared();
                        sumForce -= unit * RepulsionWeight / direction.LengthSquared();
                    }
                    if (sumForce.Length() > MinChange)
                    {
                        result[from] += sumForce;
                        hasChanged = true;
                    }
                }
            }

            return (index % 2) == 0 ? buffer : positions;
        }

        public static string Dotify(Vector2[] positions, float[,] weights)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph test {");
            sb.AppendLine("splines=\"line\";");
            for (int i = 0; i < positions.Length; ++i)
            {
                sb.AppendLine($"{i} [pos=\"{positions[i].X},{positions[i].Y}!\"]");
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
