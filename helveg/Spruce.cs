using System;
using System.Collections.Generic;
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

        public static List<Symbol> Rewrite(
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
            return src;
        }
    }
}
