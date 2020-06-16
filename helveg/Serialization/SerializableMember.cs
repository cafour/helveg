using System;
using Helveg.Analysis;

namespace Helveg.Serialization
{
    public class SerializableMember
    {
        public string? Name { get; set; }

        public Diagnosis Health { get; set; }

        public SerializableMember()
        {
        }

        public static SerializableMember FromAnalyzed(AnalyzedMember member)
        {
            return new SerializableMember
            {
                Name = member.Name,
                Health = member.Health
            };
        }

        public AnalyzedMember ToAnalyzed()
        {
            if (Name is null)
            {
                throw new NullReferenceException();
            }

            return new AnalyzedMember(Name, Health);
        }
    }
}
