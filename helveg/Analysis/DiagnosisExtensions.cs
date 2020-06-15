namespace Helveg.Analysis
{
    public static class DiagnosisExtensions
    {
        public static string ToDebuggerString(this Diagnosis diagnosis)
        {
            return diagnosis switch
            {
                Diagnosis.Error => "!",
                Diagnosis.Warning => "*",
                _ => "?",
            };
        }
    }
}
