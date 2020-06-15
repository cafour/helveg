using System;
using System.Text;

namespace Helveg.Analysis
{
    public struct AnalyzedMember : IEquatable<AnalyzedMember>
    {
        public readonly string Name;
        public readonly Diagnosis Health;

        public AnalyzedMember(string name, Diagnosis health)
        {
            Name = name;
            Health = health;
        }

        public static bool operator==(AnalyzedMember left, AnalyzedMember right)
            => left.Equals(right);

        public static bool operator!=(AnalyzedMember left, AnalyzedMember right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
        {
            if (obj is AnalyzedMember member)
            {
                return Equals(member);
            }
            return false;
        }

        public bool Equals(AnalyzedMember other)
        {
            return Name.Equals(other.Name)
                && Health.Equals(other.Health);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Health);
        }

        public override string? ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[M");
            sb.Append(Health.ToDebuggerString());
            sb.Append("] ");
            sb.Append(Name);
            return sb.ToString();
        }
    }
}
