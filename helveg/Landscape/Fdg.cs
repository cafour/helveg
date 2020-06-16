using System;
using System.Diagnostics;
using System.Numerics;

namespace Helveg.Landscape
{
    public static class Fdg
    {
        public const float DefaultGlobalSpeed = 1f;
        public const float DefaultMinGlobalSpeed = float.MinValue;
        public const float DefaultMaxGlobalSpeed = float.MaxValue;
        public const float DefaultRepulsionFactor = 2f;
        public const float DefaultOverlapRepulsionFactor = 100f;
        public const float DefaultGravityFactor = 5f;
        public const float DefaultGlobalSpeedFactor = 0.01f;
        public const float DefaultMaxSpeedConstant = 10;
        public const float DefaultNodeSize = 1f;
        public const float DefaultTraSwgRatio = 1f;

        public struct State
        {
            public Vector2[] PreviousForces;
            public Vector2[] Forces;
            public int[] Degrees;
            public float[] Swinging;
            public Vector2[] Positions;
            public float[] Weights;
            public bool PreventOverlapping;
            public bool IsGravityStrong;
            public float RepulsionFactor;
            public float GlobalSpeed;
            public float MinGlobalSpeed;
            public float MaxGlobalSpeed;
            public float OverlapRepulsionFactor;
            public float GravityFactor;
            public float GlobalSpeedFactor;
            public float MaxSpeedConstant;
            public float NodeSize;
            public float TraSwgRatio;

            public static readonly State Default = new State
            {
                GlobalSpeed = DefaultGlobalSpeed,
                MinGlobalSpeed = DefaultMinGlobalSpeed,
                MaxGlobalSpeed = DefaultMaxGlobalSpeed,
                RepulsionFactor = DefaultRepulsionFactor,
                OverlapRepulsionFactor = DefaultOverlapRepulsionFactor,
                GravityFactor = DefaultGravityFactor,
                GlobalSpeedFactor = DefaultGlobalSpeedFactor,
                MaxSpeedConstant = DefaultMaxSpeedConstant,
                NodeSize = DefaultNodeSize,
                TraSwgRatio = DefaultTraSwgRatio,
            };
        }

        public static State Create(int nodeCount, float[] weights)
        {
            var state = State.Default;
            state.PreviousForces = new Vector2[nodeCount];
            state.Forces = new Vector2[nodeCount];
            state.Swinging = new float[nodeCount];

            state.Positions = new Vector2[nodeCount];
            for (int i = 0; i < nodeCount; ++i)
            {
                var angle = 2f * MathF.PI / nodeCount * i;
                state.Positions[i] = nodeCount * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }

            state.Degrees = new int[nodeCount];
            state.Weights = weights;
            int weightIndex = 0;
            for (int from = 0; from < nodeCount; ++from)
            {
                for (int to = from + 1; to < nodeCount; ++to, ++weightIndex)
                {
                    if (state.Weights[weightIndex] > 0)
                    {
                        state.Degrees[from]++;
                        state.Degrees[to]++;
                    }
                }
            }
            return state;
        }

        public static void Step(ref State state)
        {
            {
                Vector2[] tmp = state.Forces;
                state.Forces = state.PreviousForces;
                state.PreviousForces = tmp;
            }

            int nodeCount = state.Positions.Length;

            for (int i = 0; i < nodeCount; ++i)
            {
                var direction = state.Positions[i];
                var unit = -direction / direction.Length();
                var force = state.GravityFactor * (state.Degrees[i] + 1);
                if (state.IsGravityStrong)
                {
                    force *= direction.Length();
                }
                state.Forces[i] = force * unit;
            }

            int weightIndex = 0;
            for (int from = 0; from < nodeCount; ++from)
            {
                for (int to = from + 1; to < nodeCount; ++to, ++weightIndex)
                {
                    var direction = state.Positions[to] - state.Positions[from];
                    var length = direction.Length();
                    var unit = length != 0 ? direction / length : Vector2.Zero;

                    if (state.PreventOverlapping)
                    {
                        length -= 2 * state.NodeSize;
                    }

                    float attraction = 0;
                    float repulsion = 0;
                    if (length > 0)
                    {
                        attraction = state.Weights[weightIndex] * length;
                        repulsion = state.RepulsionFactor * (state.Degrees[from] + 1)
                            * (state.Degrees[to] + 1) / length;
                    }
                    else if (length < 0)
                    {
                        repulsion = state.OverlapRepulsionFactor * (state.Degrees[from] + 1) * (state.Degrees[to] + 1);
                    }

                    state.Forces[from] += attraction * unit;
                    state.Forces[to] -= attraction * unit;
                    state.Forces[from] -= repulsion * unit;
                    state.Forces[to] += repulsion * unit;
                }
            }

            float globalTraction = 0f;
            float globalSwinging = 0f;
            for (int i = 0; i < nodeCount; ++i)
            {
                state.Swinging[i] = (state.Forces[i] - state.PreviousForces[i]).Length();
                globalSwinging += (state.Degrees[i] + 1) * state.Swinging[i];
                globalTraction += (state.Degrees[i] + 1) * (state.Forces[i] + state.PreviousForces[i]).Length() / 2f;
            }

            var globalSpeedDiff = MathF.Min(
                state.TraSwgRatio * globalTraction / globalSwinging - state.GlobalSpeed,
                state.GlobalSpeed / 2f);
            state.GlobalSpeed = Math.Clamp(
                state.MinGlobalSpeed,
                state.GlobalSpeed + globalSpeedDiff,
                state.MaxGlobalSpeed);

            for (int i = 0; i < nodeCount; ++i)
            {
                if (float.IsNaN(state.Forces[i].X) || float.IsNaN(state.Forces[i].X))
                {
                    Debugger.Break();
                }
                float speed = state.GlobalSpeedFactor * state.GlobalSpeed
                    / (1 + state.GlobalSpeed * MathF.Sqrt(state.Swinging[i]));
                var length = state.Forces[i].Length();
                if (state.PreventOverlapping)
                {
                    speed /= 10;
                    speed = MathF.Min(speed * length, 10) / (length + 1);
                }
                speed = MathF.Min(speed, state.MaxSpeedConstant / length);

                state.Positions[i] += state.Forces[i] * speed;
            }
        }
    }
}
