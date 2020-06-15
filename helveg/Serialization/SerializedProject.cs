using Helveg.Analysis;

namespace Helveg.Serialization
{
    public class SerializedProject
    {
        public string? CsprojPath { get; set; }

        public AnalyzedProject Project { get; set; }
    }
}
