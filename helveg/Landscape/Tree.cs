using System;
using System.Collections.Immutable;
using System.Linq;

namespace Helveg.Landscape
{
    public static class Tree
    {
        public enum Kind
        {
            TrunkBranching, // totalBranchCount
            Trunk, // length, width
            StemBranching, // totalBranchCount
            Canopy, // size
            YawChange,
            PitchChange,
            RollChange,
            Push,
            Pop,
        }

        public static ImmutableArray<LSymbol<Kind>> GenerateSentence(int seed, int size)
        {
            var random = new Random(seed);
            var rules = ImmutableDictionary.CreateBuilder<Kind, LRule<Kind>>();
            rules.Add(Kind.TrunkBranching, p =>
            {
                var branchCount = (int)p[0];
                if (branchCount < 2)
                {
                    return ImmutableArray.Create(
                        new LSymbol<Kind>(Kind.Trunk, 1.0f, 1.0f),
                        new LSymbol<Kind>(Kind.Canopy, 1.0f));
                }

                var currentBranchCount = branchCount;
                var right = ImmutableArray.CreateBuilder<LSymbol<Kind>>();
                right.Add(new LSymbol<Kind>(Kind.Trunk, 1.0f, 1.0f));
                var branchConfig = random.Next(1, 17);
                for (int i = 0; i < 4; ++i)
                {
                    if ((branchConfig & 1 << i) == 1 << i)
                    {
                        right.Add(new LSymbol<Kind>(Kind.Push));
                        right.Add(new LSymbol<Kind>(Kind.RollChange, MathF.PI / 2 * i));
                        right.Add(new LSymbol<Kind>(Kind.PitchChange, MathF.PI / 2));
                        var subbranchCount = random.Next(1, branchCount / 4 + 1);
                        currentBranchCount -= subbranchCount;
                        right.Add(new LSymbol<Kind>(Kind.StemBranching, subbranchCount));
                        right.Add(new LSymbol<Kind>(Kind.Pop));
                    }
                }
                right.Add(new LSymbol<Kind>(Kind.TrunkBranching, currentBranchCount));
                return right.ToImmutable();
            });
            rules.Add(Kind.StemBranching, p =>
            {
                var branchCount = (int)p[0];
                if (branchCount < 2)
                {
                    return ImmutableArray.Create(
                        new LSymbol<Kind>(Kind.Trunk, 1.0f, 1.0f),
                        new LSymbol<Kind>(Kind.Canopy, 1.0f));
                }

                var currentBranchCount = branchCount;
                var right = ImmutableArray.CreateBuilder<LSymbol<Kind>>();
                right.Add(new LSymbol<Kind>(Kind.Trunk, 1.0f, 1.0f));
                var branchConfig = random.Next(1, 5);
                for (int i = 0; i < 2; ++i)
                {
                    if ((branchConfig & 1 << i) == 1 << i)
                    {
                        right.Add(new LSymbol<Kind>(Kind.Push));
                        right.Add(new LSymbol<Kind>(Kind.YawChange, -MathF.PI / 2 + i * MathF.PI));
                        var subbranchCount = random.Next(1, branchCount / 2);
                        currentBranchCount -= subbranchCount;
                        right.Add(new LSymbol<Kind>(Kind.StemBranching, subbranchCount));
                        right.Add(new LSymbol<Kind>(Kind.Pop));
                    }
                }
                right.Add(new LSymbol<Kind>(Kind.StemBranching, currentBranchCount));
                return right.ToImmutable();

            });
            rules.Add(Kind.Trunk, p => ImmutableArray.Create(
                new LSymbol<Kind>(Kind.Trunk, p[0] * 1.5f, p[1] * 1.2f)));
            rules.Add(Kind.Canopy, p => ImmutableArray.Create(
                new LSymbol<Kind>(Kind.Canopy, p[0] * 1.5f)));
            var lsystem = new LSystem<Kind>(rules.ToImmutable());
            var sentence = ImmutableArray.Create(new LSymbol<Kind>(Kind.TrunkBranching, size));
            while (sentence.Any(s => s.Kind == Kind.TrunkBranching || s.Kind == Kind.StemBranching))
            {
                sentence = lsystem.Rewrite(sentence);
            }
            return sentence;
        }
    }
}
