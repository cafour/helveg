using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
            Trunk,
            Canopy,
            Node
        }

        [DebuggerDisplay("{Kind}({Parameter})")]
        public struct Symbol
        {
            public Kind Kind;
            public int Parameter;

            public Symbol(Kind kind, int parameter = 0)
            {
                Kind = kind;
                Parameter = parameter;
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
            int Cca(int value)
            {
                return value + random.Next(-10, 11);
            }
            var buffers = new List<Symbol>[]
            {
                new List<Symbol>(1 << 20),
                new List<Symbol>(1 << 20)
            };
            var trunkHeight = branchCount / 2;
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
                            dst.Add(new Symbol(Kind.Forward, src[i].Parameter + 1));
                            break;
                        case Kind.Trunk:
                            for (int j = trunkHeight; j > 0; --j)
                            {
                                dst.Add(new Symbol(Kind.Forward, j + 1));
                            }
                            break;
                        case Kind.Canopy:
                            var branching = Math.Min(currentBranching, branchCount);
                            var angle = 360 / branching;
                            for (int j = 0; j < branching; ++j)
                            {
                                dst.Add(new Symbol(Kind.Push));
                                dst.Add(new Symbol(Kind.RollRight, angle * j));
                                dst.Add(new Symbol(Kind.PitchDown, Cca(80)));
                                dst.Add(new Symbol(Kind.Forward, 1));
                                dst.Add(new Symbol(Kind.Node));
                                dst.Add(new Symbol(Kind.Pop));
                            }
                            branchCount -= branching;
                            currentBranching = ((currentBranching + branchingDiff - minBranching)
                                % (maxBranching - minBranching)) + minBranching;
                            dst.Add(new Symbol(Kind.Forward, 1));
                            dst.Add(new Symbol(Kind.Canopy));
                            break;
                        case Kind.Node:
                            dst.Add(new Symbol(Kind.Push));
                            dst.Add(new Symbol(Kind.TurnLeft, Cca(50)));
                            dst.Add(new Symbol(Kind.Forward, 1));
                            dst.Add(new Symbol(Kind.Node));
                            dst.Add(new Symbol(Kind.Pop));

                            dst.Add(new Symbol(Kind.Push));
                            dst.Add(new Symbol(Kind.TurnRight, Cca(50)));
                            dst.Add(new Symbol(Kind.Forward, 1));
                            dst.Add(new Symbol(Kind.Node));
                            dst.Add(new Symbol(Kind.Pop));

                            dst.Add(new Symbol(Kind.Forward, 1));
                            dst.Add(new Symbol(Kind.Node));
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
            const int polygonSize = 12;
            const float forwardLength = 7f;
            var color = new Vector3(0.4f, 0.2f, 0.0f);
            var orientation = Quaternion.CreateFromYawPitchRoll(0f, -MathF.PI / 2f, 0f);

            var vertices = new List<Vector3>();
            AddPolygon(vertices, polygonSize, sentence[0].Parameter + 1, orientation, Vector3.Zero);
            var colors = new List<Vector3>();
            colors.AddRange(Enumerable.Repeat(color, polygonSize));
            var indices = new List<uint>();

            var stack = new Queue<(Quaternion orientation, Quaternion correction, Vector3 position, int index, int lastLength)>();
            stack.Enqueue((orientation, Quaternion.Identity, Vector3.Zero, 0, polygonSize));
            while (stack.Count != 0)
            {
                var state = stack.Dequeue();
                for (; state.index < sentence.Length; ++state.index)
                {
                    var current = sentence[state.index];
                    bool shouldPop = false;
                    var parameterRad = current.Parameter / 360f * 2 * MathF.PI;
                    var forwardQ = state.orientation * new Quaternion(Vector3.UnitZ, 0)
                                * Quaternion.Conjugate(state.orientation);
                    var forward = forwardLength * new Vector3(forwardQ.X, forwardQ.Y, forwardQ.Z);

                    void AdjustOrientation(float yaw, float pitch, float roll)
                    {
                        state.orientation *= Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                        if (roll != 0) {
                            state.correction *= Quaternion.CreateFromYawPitchRoll(0, 0, -roll);
                        }
                    }
                    switch (current.Kind)
                    {
                        case Kind.Forward:
                            AddPolygon(
                                vertices,
                                polygonSize,
                                current.Parameter,
                                state.orientation * state.correction,
                                state.position + forward / 2);
                            state.position += forward;
                            colors.AddRange(Enumerable.Repeat(color, polygonSize));
                            StitchPolygons(
                                indices,
                                polygonSize,
                                (uint)(state.lastLength - polygonSize),
                                (uint)(vertices.Count - polygonSize));
                            state.lastLength = vertices.Count;
                            break;
                        case Kind.TurnLeft:
                            AdjustOrientation(parameterRad, 0f, 0f);
                            break;
                        case Kind.TurnRight:
                            AdjustOrientation(-parameterRad, 0f, 0f);
                            break;
                        case Kind.PitchUp:
                            AdjustOrientation(0f, -parameterRad, 0f);
                            break;
                        case Kind.PitchDown:
                            AdjustOrientation(0f, parameterRad, 0f);
                            break;
                        case Kind.RollRight:
                            AdjustOrientation(0f, 0f, parameterRad);
                            break;
                        case Kind.RollLeft:
                            AdjustOrientation(0f, 0f, -parameterRad);
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
                        case Kind.Node:
                        case Kind.Canopy:
                            vertices.Add(state.position + forward);
                            colors.Add(color);
                            StitchEnd(
                                indices: indices,
                                vertexCount: polygonSize,
                                polygonStart: (uint)vertices.Count - polygonSize - 1,
                                end: (uint)vertices.Count - 1);
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
            IList<uint> indices,
            uint vertexCount,
            uint startL,
            uint startR)
        {
            for (uint i = 0; i < vertexCount; ++i)
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
            IList<uint> indices,
            uint vertexCount,
            uint polygonStart,
            uint end)
        {
            for (uint i = 0; i < vertexCount; ++i)
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

            var current = new Quaternion(width / 2.0f, 0, 0, 0);
            for (int i = 0; i < vertexCount; ++i)
            {
                current = local * current * localCon;
                var global = rotation * current * rotationCon;
                vertices.Add(new Vector3(global.X, global.Y, global.Z) + center);
            }
        }
    }
}
