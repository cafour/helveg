using System;
using System.Numerics;

namespace Helveg.Landscape
{
    public static class Eades
    {
        public const float DefaultGravityFactor = 0.5f;
        public const float DefaultSpringFactor = 2f;
        public const float DefaultRepulsionFactor = 1;
        public const float DefaultSpeed = 0.1f;
        public const float DefaultSpringLengthDivisor = 1f;

        public struct State
        {
            public float[] Weights;
            public Vector2[] Forces;
            public Vector2[] Positions;
            public float GravityFactor;
            public float SpringFactor;
            public float RepulsionFactor;
            public float Speed;
            public float SpringLengthDivisor;
        }

        public static State Create(Vector2[] positions, float[] weights)
        {
            return new State
            {
                Weights = weights,
                Forces = new Vector2[positions.Length],
                Positions = positions,
                GravityFactor = DefaultGravityFactor,
                SpringFactor = DefaultSpringFactor,
                RepulsionFactor = DefaultRepulsionFactor,
                Speed = DefaultSpeed,
                SpringLengthDivisor = DefaultSpringLengthDivisor
            };
        }

        public static void Step(ref State state)
        {
            int nodeCount = state.Positions.Length;
            int weightIndex = 0;
            for (int from = 0; from < nodeCount; ++from)
            {
                var gravityDirection = -state.Positions[from];
                var gravityLength = gravityDirection.Length();
                if (gravityLength > 0f)
                {
                    state.Forces[from] = state.GravityFactor * gravityDirection / gravityLength;
                }

                for (int to = from + 1; to < nodeCount; ++to, ++weightIndex)
                {
                    var direction = state.Positions[to] - state.Positions[from];
                    var lengthSquared = direction.LengthSquared();
                    var length = MathF.Sqrt(lengthSquared);
                    if (length > 0f)
                    {
                        var unit = direction / length;
                        var force = state.Weights[weightIndex] > 0f
                            ? state.SpringFactor * MathF.Log2(length / state.SpringLengthDivisor)
                            : -state.RepulsionFactor / lengthSquared;
                        force = Math.Clamp(force, -nodeCount, nodeCount);
                        state.Forces[from] += force * unit;
                        state.Forces[to] -= force * unit;
                    }
                }
            }
            for (int i = 0; i < nodeCount; ++i)
            {
                state.Positions[i] += state.Speed * state.Forces[i];
            }
        }
    }
}
