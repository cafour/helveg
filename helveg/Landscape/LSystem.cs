using System;
using System.Collections.Immutable;

namespace Helveg.Landscape
{
    public struct LSystem<TKind>
        where TKind : Enum
    {
        public LSystem(ImmutableDictionary<TKind, LRule<TKind>> rules)
        {
            Rules = rules;
        }

        public ImmutableDictionary<TKind, LRule<TKind>> Rules { get; }

        public ImmutableArray<LSymbol<TKind>> Rewrite(ImmutableArray<LSymbol<TKind>> sentence)
        {
            var builder = ImmutableArray.CreateBuilder<LSymbol<TKind>>();
            for (int i = 0; i < sentence.Length; ++i)
            {
                if (Rules.TryGetValue(sentence[i].Kind, out var right))
                {
                    builder.AddRange(right(sentence[i].Parameters));
                }
                else
                {
                    builder.Add(sentence[i]);
                }
            }
            return builder.ToImmutable();
        }
    }
}
