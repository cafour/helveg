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

        public static void StepForceAtlas2(
            Vector2[] previousForces,
            Vector2[] forces,
            int[] deg,
            float[] swinging,
            ref float globalSpeed,
            Vector2[] positions,
            float[,] weights,
            bool preventOverlapping = false)
        {
            int nodeCount = positions.Length;

            const float repulsionFactor = 1f;
            const float overlapRepulsionFactor = 100f;
            const float gravityFactor = 1f;
            const float globalSpeedFactor = 0.1f;
            const float maxSpeedConstant = 10;
            const float nodeSize = 1f;
            const float traSwgRatio = 1f;

            for (int i = 0; i < nodeCount; ++i)
            {
                var direction = positions[i];
                var unit = -direction / direction.Length();
                var force = gravityFactor * (deg[i] + 1);
                forces[i] = force * unit;
            }

            for (int from = 0; from < nodeCount; ++from)
            {
                for (int to = from + 1; to < nodeCount; ++to)
                {
                    var direction = positions[to] - positions[from];
                    var length = direction.Length();
                    var unit = length != 0 ? direction / length : Vector2.Zero;

                    if (preventOverlapping)
                    {
                        length -= 2 * nodeSize;
                    }

                    float weight = weights[from, to] + weights[to, from];
                    var attraction = weight * length;
                    forces[from] += attraction * unit;
                    forces[to] -= attraction * unit;

                    float repulsion = 0;
                    if (length > 0)
                    {
                        repulsion = repulsionFactor * (deg[from] + 1) * (deg[to] + 1) / length;
                    }
                    else if (length < 0)
                    {
                        repulsion = overlapRepulsionFactor * (deg[from] + 1) * (deg[to] + 1);
                    }

                    forces[from] -= repulsion * unit;
                    forces[to] += repulsion * unit;
                }
            }

            float globalTraction = 0f;
            float globalSwinging = 0f;
            for (int i = 0; i < nodeCount; ++i)
            {
                swinging[i] = (forces[i] - previousForces[i]).Length();
                globalSwinging += (deg[i] + 1) * swinging[i];
                globalTraction += (deg[i] + 1) * (forces[i] + previousForces[i]).Length() / 2f;
            }

            globalSpeed += MathF.Max(traSwgRatio * globalTraction / globalSwinging - globalSpeed, globalSpeed / 2f);

            for (int i = 0; i < nodeCount; ++i)
            {
                float speed = globalSpeedFactor * globalSpeed / (1 + globalSpeed * MathF.Sqrt(swinging[i]));
                speed = MathF.Max(speed, maxSpeedConstant / forces[i].Length());
                forces[i] *= speed;
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
