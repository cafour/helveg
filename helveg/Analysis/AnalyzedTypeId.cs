using System;

namespace Helveg
{
    public struct AnalyzedTypeId
    {
        public readonly string Name;
        public readonly string Namespace;

        public static readonly AnalyzedTypeId Dynamic = new AnalyzedTypeId("dynamic", "global");

        public static readonly AnalyzedTypeId Error = new AnalyzedTypeId("error", "global");

        public AnalyzedTypeId(string name, string @namespace, int arity = 0)
        {
            Name = arity > 0 ? $"{name}`{arity}" : name;
            Namespace = @namespace;
        }

        public AnalyzedTypeId(string name, AnalyzedTypeId parent, int arity = 0)
            : this($"{parent.Name}.{name}", parent.Namespace, arity)
        {
        }

        public static bool operator ==(AnalyzedTypeId left, AnalyzedTypeId right)
            => left.Equals(right);

        public static bool operator !=(AnalyzedTypeId left, AnalyzedTypeId right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
        {
            if (obj is AnalyzedTypeId member)
            {
                return Equals(member);
            }
            return false;
        }

        public bool Equals(AnalyzedTypeId other)
        {
            return Name.Equals(other.Name)
                && Namespace.Equals(other.Namespace);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Namespace);
        }

        public override string? ToString()
        {
            return $"{Namespace}::{Name}";
        }
    }
}
