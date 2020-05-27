using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Helveg
{
    public static class Spruce
    {
        public enum Kind : byte
        {
            Forward,
            TurnLeft,
            TurnRight,
            PitchUp,
            PitchDown,
            RollRight,
            RollLeft,
            Push,
            Pop,
            Canopy,
            Branch
        }

        [DebuggerDisplay("{Kind}({Parameter})")]
        [StructLayout(LayoutKind.Explicit)]
        public struct Symbol
        {
            [FieldOffset(0)] public Kind Kind;
            [FieldOffset(sizeof(byte))] public int Int;
            [FieldOffset(sizeof(byte))] public float Float;

            public Symbol(Kind kind, int @int = 0)
            {
                Kind = kind;
                Float = 0;
                Int = @int;
            }

            public Symbol(Kind kind, float @float)
            {
                Kind = kind;
                Int = 0;
                Float = @float;
            }
        }

        public static Symbol[] Rewrite(
            IEnumerable<Symbol> axiom,
            int seed,
            int branchCount,
            int maxBranching,
            int minBranching,
            int initialBranching,
            int branchingDiff)
        {
            var random = new Random(seed);
            float Cca(float value)
            {
                return value + random.Next(-10, 11) * MathF.PI / 180f;
            }

            var buffers = new List<Symbol>[]
            {
                new List<Symbol>(1 << 20),
                new List<Symbol>(1 << 20)
            };
            var currentBranching = initialBranching;
            var src = buffers[0];
            src.AddRange(axiom);
            var dst = buffers[1];
            var index = 0;
            while (branchCount > 0)
            {
                dst.Clear();
                for (int i = 0; i < src.Count; ++i)
                {
                    switch (src[i].Kind)
                    {
                        case Kind.Forward:
                            dst.Add(new Symbol(Kind.Forward, src[i].Int + 1));
                            break;
                        case Kind.Canopy:
                            dst.Add(new Symbol(Kind.Forward, 1));
                            var branching = Math.Min(currentBranching, branchCount);
                            var angle = 2 * MathF.PI / branching;
                            for (int j = 0; j < branching; ++j)
                            {
                                dst.Add(new Symbol(Kind.Push));
                                dst.Add(new Symbol(Kind.RollRight, angle * j));
                                dst.Add(new Symbol(Kind.PitchDown, Cca(1.6f)));
                                dst.Add(new Symbol(Kind.Branch));
                                dst.Add(new Symbol(Kind.Pop));
                            }
                            branchCount -= branching;
                            currentBranching = ((currentBranching + branchingDiff - minBranching)
                                % (maxBranching - minBranching)) + minBranching;
                            dst.Add(new Symbol(Kind.Canopy));
                            break;
                        case Kind.Branch:
                            dst.Add(new Symbol(Kind.Forward, 1));

                            dst.Add(new Symbol(Kind.Push));
                            dst.Add(new Symbol(Kind.TurnLeft, Cca(0.9f)));
                            dst.Add(new Symbol(Kind.Branch));
                            dst.Add(new Symbol(Kind.Pop));

                            dst.Add(new Symbol(Kind.Push));
                            dst.Add(new Symbol(Kind.TurnRight, Cca(0.9f)));
                            dst.Add(new Symbol(Kind.Branch));
                            dst.Add(new Symbol(Kind.Pop));

                            dst.Add(new Symbol(Kind.Branch));
                            break;
                        default:
                            dst.Add(src[i]);
                            break;
                    }
                }

                dst = buffers[index];
                index = (index + 1) % 2;
                src = buffers[index];
            }
            return src.ToArray();
        }

        public static Mesh GenerateMesh(Symbol[] sentence)
        {
            const int edgeCount = 12;
            var color = new Vector3(0.4f, 0.2f, 0.0f);
            var orientation = Quaternion.CreateFromYawPitchRoll(0f, -MathF.PI / 2f, 0f);

            var vertices = new List<Vector3>();
            AddPolygon(vertices, edgeCount, sentence[0].Int + 1, orientation, Vector3.Zero);
            var colors = new List<Vector3>();
            colors.AddRange(Enumerable.Repeat(color, edgeCount));
            var indices = new List<int>();

            var stack = new Queue<(Quaternion orientation, Quaternion correction, Vector3 position, int index, int joint)>();
            stack.Enqueue((orientation, Quaternion.Identity, Vector3.Zero, 0, 0));
            while (stack.Count != 0)
            {
                var state = stack.Dequeue();
                for (; state.index < sentence.Length; ++state.index)
                {
                    var current = sentence[state.index];
                    bool shouldPop = false;
                    var forwardQ = state.orientation * new Quaternion(Vector3.UnitZ, 0)
                                * Quaternion.Conjugate(state.orientation);
                    var forward = new Vector3(forwardQ.X, forwardQ.Y, forwardQ.Z);

                    void AdjustOrientation(float yaw, float pitch, float roll)
                    {
                        state.orientation *= Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                        if (roll != 0)
                        {
                            state.correction *= Quaternion.CreateFromYawPitchRoll(0, 0, -roll);
                        }
                    }
                    switch (current.Kind)
                    {
                        case Kind.Forward:
                            float length = (1 << current.Int - 1) * 0.9f;
                            AddPolygon(
                                vertices,
                                edgeCount,
                                current.Int,
                                state.orientation * state.correction,
                                state.position + forward * length * 0.667f );
                            state.position += forward * length;
                            colors.AddRange(Enumerable.Repeat(color, edgeCount));
                            StitchPolygons(
                                indices,
                                edgeCount,
                                state.joint,
                                vertices.Count - edgeCount);
                            state.joint = vertices.Count - edgeCount;
                            break;
                        case Kind.TurnLeft:
                            AdjustOrientation(current.Float, 0f, 0f);
                            break;
                        case Kind.TurnRight:
                            AdjustOrientation(-current.Float, 0f, 0f);
                            break;
                        case Kind.PitchUp:
                            AdjustOrientation(0f, -current.Float, 0f);
                            break;
                        case Kind.PitchDown:
                            AdjustOrientation(0f, current.Float, 0f);
                            break;
                        case Kind.RollRight:
                            AdjustOrientation(0f, 0f, current.Float);
                            break;
                        case Kind.RollLeft:
                            AdjustOrientation(0f, 0f, -current.Float);
                            break;
                        case Kind.Push:
                            state.index++;
                            stack.Enqueue(state);
                            int nest = 1;
                            for (int i = state.index; i < sentence.Length; ++i)
                            {
                                if (sentence[i].Kind == Kind.Push)
                                {
                                    nest++;
                                }
                                else if (sentence[i].Kind == Kind.Pop)
                                {
                                    nest--;
                                    if (nest == 0)
                                    {
                                        state.index = i;
                                        break;
                                    }
                                }
                            }
                            if (nest != 0)
                            {
                                throw new InvalidOperationException($"Push at {state.index} is missing a Pop.");
                            }
                            break;
                        case Kind.Pop:
                            shouldPop = true;
                            break;
                        case Kind.Branch:
                        case Kind.Canopy:
                            vertices.Add(state.position + forward * 0.5f);
                            colors.Add(color);
                            StitchEnd(
                                indices: indices,
                                vertexCount: edgeCount,
                                polygonStart: state.joint,
                                end: vertices.Count - 1);
                            break;
                        default:
                            break;
                    }
                    if (shouldPop)
                    {
                        break;
                    }
                }
            }

            return new Mesh(vertices.ToArray(), colors.ToArray(), indices.ToArray());
        }

        private static void StitchPolygons(
            IList<int> indices,
            int vertexCount,
            int startL,
            int startR)
        {
            for (int i = 0; i < vertexCount; ++i)
            {
                indices.Add(startL + i);
                indices.Add(startL + (i + 1) % vertexCount);
                indices.Add(startR + i);
                indices.Add(startR + i);
                indices.Add(startL + (i + 1) % vertexCount);
                indices.Add(startR + (i + 1) % vertexCount);
            }
        }

        private static void StitchEnd(
            IList<int> indices,
            int vertexCount,
            int polygonStart,
            int end)
        {
            for (int i = 0; i < vertexCount; ++i)
            {
                indices.Add(polygonStart + i);
                indices.Add(polygonStart + (i + 1) % vertexCount);
                indices.Add(end);
            }
        }

        private static void AddPolygon(
            IList<Vector3> vertices,
            int vertexCount,
            int width,
            Quaternion rotation,
            Vector3 center)
        {
            var rotationCon = Quaternion.Conjugate(rotation);
            var local = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 2 * MathF.PI / vertexCount);
            var localCon = Quaternion.Conjugate(local);

            var current = new Quaternion(width / 12f, 0, 0, 0);
            for (int i = 0; i < vertexCount; ++i)
            {
                current = local * current * localCon;
                var global = rotation * current * rotationCon;
                vertices.Add(new Vector3(global.X, global.Y, global.Z) + center);
            }
        }
    }
}
