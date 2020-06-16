using System;
using System.Numerics;

namespace Helveg.Landscape
{
    public static class Eades
    {
        public struct State
        {
            public float[] Weights;
            public Vector2[] Forces;
            public Vector2[] Positions;
        }

        public static State Create(Vector2[] positions, float[] weights)
        {
            return new State
            {
                Weights = weights,
                Forces = new Vector2[positions.Length],
                Positions = positions
            };
        }

        public static void Step(ref State state)
        {
            int nodeCount = state.Positions.Length;
            int weightIndex = 0;
            for (int from = 0; from < nodeCount; ++from)
            {
                var centerDistance = state.Positions[from].Length();
                state.Forces[from] = -0.5f * state.Positions[from] / centerDistance * MathF.Log2(centerDistance + 1);
                for (int to = from + 1; to < nodeCount; ++to, ++weightIndex)
                {
                    var direction = state.Positions[to] - state.Positions[from];
                    var length = direction.Length();
                    var unit = direction / length;
                    var force = state.Weights[weightIndex] > 0f
                        ? 2f * MathF.Log2(length)
                        : -1f / (length * length);
                    force = Math.Clamp(force, -nodeCount, nodeCount);
                    state.Forces[from] += force * unit;
                    state.Forces[to] -= force * unit;
                }
            }
            for (int i = 0; i < nodeCount; ++i)
            {
                state.Positions[i] += 0.1f * state.Forces[i];
            }
        }
    }
}
