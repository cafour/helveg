using System.Collections.Immutable;

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

        // public static ImmutableArray<LSymbol<Kind>> Generate(int seed, int size)
        // {
        //     var rules = ImmutableDictionary.CreateBuilder<Kind, LRule<Kind>>();
        //     rules.Add(p => );
        //     var lsystem = new LSystem<Kind>(rules.ToImmutable());
        //     var sentence = ImmutableArray.Create(new LSymbol<Kind>(Kind.TrunkBranching, size));
        //     while (sentence.Any(s => s.Kind == Kind.TrunkBranching || s.Kind == Kind.StemBranching))
        //     {
        //         sentence = lsystem.Rewrite(sentence);
        //     }
        //     return sentence;
        // }
    }
}
