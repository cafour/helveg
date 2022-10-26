using System;
using System.Numerics;

namespace Helveg.Landscape
{
    public static class Eades
    {
        public const float DefaultGravity = 1f;
        public const float DefaultStiffness = 2f;
        public const float DefaultRepulsion = 1f;
        public const float DefaultSpeed = 0.1f;
        public const float DefaultUnloadedLength = 1f;

        public struct State
        {
            public float[] Weights;
            public Vector2[] Forces;
            public Vector2[] Positions;
            public float Gravity;
            public float Stiffness;
            public float Repulsion;
            public float Speed;
            public float UnloadedLength;
        }

        public static State Create(Vector2[] positions, float[] weights)
        {
            return new State
            {
                Weights = weights,
                Forces = new Vector2[positions.Length],
                Positions = positions,
                Gravity = DefaultGravity,
                Stiffness = DefaultStiffness,
                Repulsion = DefaultRepulsion,
                Speed = DefaultSpeed,
                UnloadedLength = DefaultUnloadedLength
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
                state.Forces[from] = state.Gravity * gravityDirection / MathF.Max(1f, gravityLength);

                for (int to = from + 1; to < nodeCount; ++to, ++weightIndex)
                {
                    var direction = state.Positions[to] - state.Positions[from];
                    // prevent division by 0 by pushing nodes apart
                    if (direction.X == 0f && direction.Y == 0f)
                    {
                        var push = new Vector2(float.Epsilon, float.Epsilon);
                        state.Positions[from] += push;
                        state.Positions[to] -= push;
                    }
                    var lengthSquared = direction.LengthSquared();
                    var length = MathF.Sqrt(lengthSquared);
                    var unit = direction / length;
                    var force = state.Weights[weightIndex] > 0f
                        ? state.Stiffness * MathF.Log2(length / state.UnloadedLength)
                        : -state.Repulsion / lengthSquared;
                    force = Math.Clamp(force, -nodeCount, nodeCount);
                    state.Forces[from] += force * unit;
                    state.Forces[to] -= force * unit;
                }
            }
            for (int i = 0; i < nodeCount; ++i)
            {
                state.Positions[i] += state.Speed * state.Forces[i];
            }
        }
    }
}
