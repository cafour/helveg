using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Helveg.Landscape
{
    public struct LSymbol<TKind> : IEquatable<LSymbol<TKind>>
        where TKind : Enum
    {
        public TKind Kind;
        public ImmutableArray<float> Parameters;

        public LSymbol(TKind kind, params float[] parameters)
        {
            Kind = kind;
            Parameters = parameters.ToImmutableArray();
        }

        public static bool operator ==(LSymbol<TKind> left, LSymbol<TKind> right)
            => left.Equals(right);

        public static bool operator !=(LSymbol<TKind> left, LSymbol<TKind> right)
            => !left.Equals(right);

        public override string ToString()
        {
            var name = Kind.ToString();
            if (name == "Push")
            {
                return "[";
            }
            else if (name == "Pop")
            {
                return "]";
            }

            var sb = new StringBuilder();
            foreach (var initial in name.Where(c => c <= 'Z'))
            {
                sb.Append(initial);
            }
            sb.Append('(');
            sb.Append(string.Join(", ", Parameters));
            sb.Append(')');
            return sb.ToString();
        }

        public bool Equals(LSymbol<TKind> other)
        {
            if (Parameters.Length != other.Parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < Parameters.Length; ++i)
            {
                if (Parameters[i] != other.Parameters[i])
                {
                    return false;
                }
            }
            return Kind.Equals(other.Kind);
        }

        public override bool Equals(object? obj)
        {
            if (obj is LSymbol<TKind> symbol)
            {
                return Equals(symbol);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Parameters.Length);
        }
    }
}
