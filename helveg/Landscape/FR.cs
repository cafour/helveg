using System;
using System.Numerics;

namespace Helveg.Landscape
{
    public static class FR
    {
        public struct State
        {
            public float Width;
            public float Height;
            public float Temperature;
            public float K;
            public float[] Weights;
            public Vector2[] Positions;
            public Vector2[] Displacement;
        }

        public static State Create(int nodeCount, float[] weights, float width, float height)
        {
            var area = width * height;
            return new State
            {
                Width = width,
                Height = height,
                K = MathF.Sqrt(area / nodeCount) / 3f,
                Temperature = area / 100f,
                Weights = weights,
                Positions = new Vector2[nodeCount],
                Displacement = new Vector2[nodeCount]
            };
        }

        public static void Step(ref State state)
        {
            int nodeCount = state.Positions.Length;

            // calculate repulsive forces
            for (int from = 0; from < nodeCount; ++from)
            {
                state.Displacement[from] = Vector2.Zero;
                for (int to = 0; to < nodeCount; ++to)
                {
                    if (from == to)
                    {
                        continue;
                    }

                    var direction = state.Positions[from] - state.Positions[to];
                    var length = direction.Length();
                    if (length > 0)
                    {
                        var unit = direction / length;
                        var force = state.K * state.K / length;
                        state.Displacement[from] += force * unit;
                    }
                }
            }

            // calculate attractive forces
            int weightIndex = 0;
            for (int from = 0; from < nodeCount; ++from)
            {
                for (int to = from + 1; to < nodeCount; ++to, ++weightIndex)
                {
                    if (state.Weights[weightIndex] > 0)
                    {
                        var direction = state.Positions[from] - state.Positions[to];
                        var length = direction.Length();
                        if (length > 0)
                        {
                            var unit = direction / length;
                            var force = length * length / state.K;
                            state.Displacement[from] -= unit * force;
                            state.Displacement[to] += unit * force;
                        }
                    }
                }
            }

            // limit max displacement to temperature 
            for (int i = 0; i < nodeCount; ++i)
            {
                var length = state.Displacement[i].Length();
                state.Positions[i] += state.Displacement[i] / length * MathF.Min(length, state.Temperature);
                state.Positions[i].X = MathF.Min(state.Width / 2f, MathF.Max(-state.Width / 2f, state.Positions[i].X));
                state.Positions[i].Y = MathF.Min(state.Height / 2f, MathF.Max(-state.Height / 2f, state.Positions[i].Y));
            }
            state.Temperature *= 0.9f;
        }
    }
}
