using System;
using System.Collections.Immutable;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Landscape
{
    public static class Spruce
    {
        public enum Kind
        {
            YawChange,
            PitchChange,
            RollChange,
            Forward,
            Push,
            Pop,
            ZerothOrder,
            FirstOrder,
            SecondOrder,
            ThirdOrder
        }

        public static ImmutableArray<LSymbol<Kind>> Generate(int seed, int size)
        {
            const float length0 = 24f;
            const float length1 = 16f;
            const float length2 = 12f;
            const float length3 = 8f;
            const float defaultRadius = 0.5f;
            const float volTransfer0 = 0.18f;
            const float volTransfer1 = 0.95f;


            var rules = ImmutableDictionary.CreateBuilder<Kind, LRule<Kind>>();
            rules.Add(Kind.ZerothOrder, p =>
            {
                var builder = ImmutableArray.CreateBuilder<LSymbol<Kind>>();
                builder.Add(new LSymbol<Kind>(Kind.Forward, length0, defaultRadius));
                var branchCount = Math.Min(size, 5);
                size -= branchCount;
                if (branchCount > 0)
                {
                    for (int i = 0; i < branchCount; ++i)
                    {
                        builder.Add(new LSymbol<Kind>(Kind.Push));
                        builder.Add(new LSymbol<Kind>(Kind.RollChange, i * MathF.PI * 0.4f));
                        builder.Add(new LSymbol<Kind>(Kind.PitchChange, MathF.PI * 0.42f));
                        builder.Add(new LSymbol<Kind>(Kind.FirstOrder));
                        builder.Add(new LSymbol<Kind>(Kind.Pop));
                    }
                }
                builder.Add(new LSymbol<Kind>(Kind.ZerothOrder));
                return builder.ToImmutable();
            });
            rules.Add(Kind.FirstOrder, p => ImmutableArray.Create(
                new LSymbol<Kind>(Kind.Forward, length1, defaultRadius),
                new LSymbol<Kind>(Kind.Push),
                new LSymbol<Kind>(Kind.YawChange, MathF.PI * 0.4f),
                new LSymbol<Kind>(Kind.SecondOrder),
                new LSymbol<Kind>(Kind.Pop),
                new LSymbol<Kind>(Kind.Push),
                new LSymbol<Kind>(Kind.FirstOrder),
                new LSymbol<Kind>(Kind.Pop),
                new LSymbol<Kind>(Kind.Push),
                new LSymbol<Kind>(Kind.YawChange, -MathF.PI * 0.4f),
                new LSymbol<Kind>(Kind.SecondOrder),
                new LSymbol<Kind>(Kind.Pop)));
            rules.Add(Kind.SecondOrder, p => ImmutableArray.Create(
                new LSymbol<Kind>(Kind.Forward, length2, defaultRadius),
                new LSymbol<Kind>(Kind.Push),
                new LSymbol<Kind>(Kind.YawChange, MathF.PI * 0.4f),
                new LSymbol<Kind>(Kind.ThirdOrder),
                new LSymbol<Kind>(Kind.Pop),
                new LSymbol<Kind>(Kind.Push),
                new LSymbol<Kind>(Kind.SecondOrder),
                new LSymbol<Kind>(Kind.Pop),
                new LSymbol<Kind>(Kind.Push),
                new LSymbol<Kind>(Kind.YawChange, -MathF.PI * 0.4f),
                new LSymbol<Kind>(Kind.ThirdOrder),
                new LSymbol<Kind>(Kind.Pop)));
            rules.Add(Kind.ThirdOrder, p =>
                ImmutableArray.Create(new LSymbol<Kind>(Kind.Forward, length3, defaultRadius)));
            var lsystem = new LSystem<Kind>(rules.ToImmutable());
            var sentence = ImmutableArray.Create(
                new LSymbol<Kind>(Kind.PitchChange, -MathF.PI / 2f),
                new LSymbol<Kind>(Kind.ZerothOrder));
            while (size != 0)
            {
                sentence = lsystem.Rewrite(sentence);
                var rebuilder = sentence.ToBuilder();
                for (int i = 0; i < rebuilder.Count; ++i)
                {
                    float volume = rebuilder[i].Kind switch
                    {
                        Kind.ZerothOrder => length0 * MathF.PI,
                        Kind.FirstOrder => length1 * MathF.PI,
                        Kind.SecondOrder => length2 * MathF.PI,
                        Kind.ThirdOrder => length3 * MathF.PI,
                        _ => 0f
                    };
                    if (volume == 0f)
                    {
                        continue;
                    }

                    volume *= volTransfer0;

                    int depth = 0;
                    for (int j = i - 1; j >= 0; --j)
                    {
                        if (rebuilder[j].Kind == Kind.Pop)
                        {
                            depth++;
                        }
                        else if (rebuilder[j].Kind == Kind.Push)
                        {
                            depth = Math.Max(depth - 1, 0);
                        }
                        else if (depth == 0 && rebuilder[j].Kind == Kind.Forward)
                        {
                            float newRadius = MathF.Sqrt(rebuilder[j].Parameters[1] * rebuilder[j].Parameters[1]
                                + volume / MathF.PI / rebuilder[j].Parameters[0]);
                            rebuilder[j] = new LSymbol<Kind>(
                                Kind.Forward,
                                rebuilder[j].Parameters[0],
                                newRadius);
                            volume *= volTransfer1;
                        }
                    }
                }
                sentence = rebuilder.ToImmutable();
            }
            return sentence;
        }

        public static void Draw(
            ImmutableArray<LSymbol<Kind>> sentence,
            WorldBuilder world,
            Point3 position,
            Block wood,
            Block leaves)
        {
            var drawRules = ImmutableDictionary.CreateBuilder<Kind, LDraw>();
            drawRules.Add(Kind.Forward, (s, w) =>
            {
                var to = s.Position + Point3.Round(s.Forward * s.Parameters[0]);
                w.OverLine(s.Position, to, p => w.FillSphere(p, wood, (int)MathF.Round(s.Parameters[1] + 0.01f)));
                return to;
            });
            var turtle = new LTurtle<Kind>(
                push: Kind.Push,
                pop: Kind.Pop,
                yawChange: Kind.YawChange,
                pitchChange: Kind.PitchChange,
                rollChange: Kind.RollChange,
                drawRules: drawRules.ToImmutable());
            var orientation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f);
            turtle.Draw(sentence, world, position, orientation);
        }
    }
}
