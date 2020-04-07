using System;
using System.Collections.Generic;
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
            int branchCount,
            int maxBranching,
            int minBranching,
            int initialBranching,
            int branchingDiff)
        {
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
                                dst.Add(new Symbol(Kind.PitchDown, 80));
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
                            dst.Add(new Symbol(Kind.TurnLeft, 50));
                            dst.Add(new Symbol(Kind.Forward, 1));
                            dst.Add(new Symbol(Kind.Node));
                            dst.Add(new Symbol(Kind.Pop));

                            dst.Add(new Symbol(Kind.Push));
                            dst.Add(new Symbol(Kind.TurnRight, 50));
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
            const float forwardLength = 10f;

            var vertices = new List<Vector3>();
            AddPolygon(vertices, polygonSize, 10, Quaternion.Identity, Vector3.Zero);
            var colors = new List<Vector3>();
            colors.AddRange(Enumerable.Repeat(Vector3.Zero, polygonSize));
            var indices = new List<uint>();

            var stack = new Stack<(int yaw, int pitch, int roll, Vector3 position, int index)>();
            stack.Push((0, 270, 0, new Vector3(0, 0, 0), 0));
            while (stack.Count != 0)
            {
                var state = stack.Pop();
                for (; state.index < sentence.Length; ++state.index)
                {
                    var current = sentence[state.index];
                    bool shouldPop = false;
                    switch (current.Kind)
                    {
                        case Kind.Forward:
                            var rotation = Quaternion.CreateFromYawPitchRoll(
                                yaw: state.yaw / 360f * 2 * MathF.PI,
                                pitch: state.pitch / 360f * 2 * MathF.PI,
                                roll: state.roll / 360f * 2 * MathF.PI);
                            var forwardQ = rotation * new Quaternion(Vector3.UnitZ, 0)
                                * Quaternion.Conjugate(rotation);
                            var forward = forwardLength * new Vector3(forwardQ.X, forwardQ.Y, forwardQ.Z);
                            state.position += forward;
                            AddPolygon(
                                vertices,
                                polygonSize,
                                current.Parameter,
                                rotation,
                                state.position + forward / 2);
                            colors.AddRange(Enumerable.Repeat(new Vector3(0.6f, 0.4f, 0.0f), polygonSize));
                            StitchPolygons(
                                indices,
                                polygonSize,
                                (uint)(vertices.Count - 2 * polygonSize),
                                (uint)(vertices.Count - polygonSize));
                            break;
                        case Kind.TurnLeft:
                            state.yaw = (state.yaw + current.Parameter) % 360;
                            break;
                        case Kind.TurnRight:
                            state.yaw = (state.yaw - current.Parameter) % 360;
                            break;
                        case Kind.PitchUp:
                            state.yaw = (state.pitch - current.Parameter) % 360;
                            break;
                        case Kind.PitchDown:
                            state.yaw = (state.pitch + current.Parameter) % 360;
                            break;
                        case Kind.RollRight:
                            state.yaw = (state.roll + current.Parameter) % 360;
                            break;
                        case Kind.RollLeft:
                            state.yaw = (state.roll - current.Parameter) % 360;
                            break;
                        case Kind.Push:
                            state.index++;
                            stack.Push(state);
                            int j = state.index;
                            for (; sentence[j].Kind != Kind.Pop; ++j)
                            {
                                if (j == sentence.Length - 1)
                                {
                                    throw new InvalidOperationException($"Push at {state.index} is missing a Pop.");
                                }
                            }
                            state.index = j + 1;
                            break;
                        case Kind.Pop:
                            shouldPop = true;
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
            int vertexCount,
            uint startL,
            uint startR)
        {
            for (uint i = 0; i < vertexCount; ++i)
            {
                uint l = startL + i;
                uint r = startR + i;
                indices.Add(l);
                indices.Add(l + 1);
                indices.Add(r);
                indices.Add(r);
                indices.Add(l + 1);
                indices.Add(r + 1);
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
            var local = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), 2 * MathF.PI / vertexCount);
            var localCon = Quaternion.Conjugate(local);

            var current = new Quaternion(width, 0, 0, 0);
            for (int i = 0; i < vertexCount; ++i)
            {
                current = local * current * localCon;
                var global = rotation * current * rotationCon;
                vertices.Add(new Vector3(global.X, global.Y, global.Z) + center);
            }
        }
    }
}
