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

        // 15 krokov malých skokov 
        // Po kameňoch starých mlokov
        // V pravo rieka v ľavo more
        // Na rozcestí rovno hore
        // Do chalúpky ku babičke 
        // Od nej krátko k starej líške
        // Do jaskyňe krátkozraka 
        // Pozor však ak vrana kráka
        // Za koláčik do nory
        // Ľahko prejdeš cez hory
        // Potom už len malý krôčik
        // Domov príď cez potôčik

        public static void ApplyForces(Vector2[] positions, float[,] weights, int maxIterations)
        {
            int nodeCount = positions.Length;
            if (nodeCount != weights.GetLength(0)
                || nodeCount != weights.GetLength(1))
            {
                throw new ArgumentException("The lengths of arguments do not match.");
            }

            Vector2[] attraction = new Vector2[nodeCount];
            Vector2[] repulsion = new Vector2[nodeCount];
            for (int index = 0; index < maxIterations; ++index)
            {
                const float diameter = 2f;
                const float weightValue = 1f;
                // Edge attraction
                for (int from = 0; from < nodeCount; ++from)
                {
                    var centerDistance = positions[from].Length();
                    attraction[from] = -0.01f * positions[from] / centerDistance * MathF.Log2(centerDistance + 1);

                    // for (int to = 0; to < nodeCount; ++to)
                    // {
                    //     if (from == to)
                    //     {
                    //         continue;
                    //     }

                    //     // linear: https://www.desmos.com/calculator/7ngbnfmcic
                    //     // logarithmic: https://www.desmos.com/calculator/4i201dkpqm
                    //     var direction = positions[to] - positions[from];
                    //     var length = direction.Length();
                    //     var unit = direction / length;
                    //     var weight = weights[from, to] + weights[to, from] > 0 ? weightValue : 0f;

                    //     attraction[from] += weight * unit * MathF.Log2(length + 1);
                    // }
                }

                // Node repulsion
                // for (int from = 0; from < nodeCount; ++from)
                // {
                //     repulsion[from] = Vector2.Zero;
                //     for (int to = 0; to < nodeCount; ++to)
                //     {
                //         if (from == to)
                //         {
                //             continue;
                //         }

                //         var direction = positions[to] - positions[from];
                //         var length = direction.Length();
                //         var unit = direction / length;
                //         var relevant = Vector2.Dot(unit, attraction[from]);
                //         repulsion[from] += unit * Math.Min(0, length - diameter - relevant);
                //     }
                // }
                // for (int from = 0; from < nodeCount; ++from)
                // {
                //     Vector2 sumForce = Vector2.Zero;
                //     for (int to = 0; to < nodeCount; ++to)
                //     {
                //         if (from == to)
                //         {
                //             continue;
                //         }

                //         const float diameter = 2f;
                //         const float weightValue = 1f;
                //         var direction = positions[to] - positions[from];
                //         var length = direction.Length();
                //         var unit = direction / length;
                //         var weight = weights[from, to] + weights[to, from] > 0 ? weightValue : 0f;

                //         sumForce += weight * unit * MathF.Log2(length + 1);
                //         sumForce -= Math.Max(0f, -(length * length) + diameter * diameter + weight * MathF.Log2(diameter + 1)) * unit;

                //         var centerDistance = positions[from].Length();
                //         sumForce += -0.001f * positions[from] / centerDistance * MathF.Log2(centerDistance + 1);
                //     }
                //     forces[from] = sumForce * 0.1f;
                // }
                for (int i = 0; i < nodeCount; ++i)
                {
                    positions[i] += attraction[i] + repulsion[i];
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
