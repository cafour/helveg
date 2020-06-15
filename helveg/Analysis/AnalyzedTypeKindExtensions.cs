namespace Helveg.Analysis
{
    public static class AnalyzedTypeKindExtensions
    {
        public static string ToDebuggerString(this AnalyzedTypeKind kind)
        {
            return kind switch
            {
                AnalyzedTypeKind.Class => "c",
                AnalyzedTypeKind.Struct => "s",
                AnalyzedTypeKind.Interface => "i",
                AnalyzedTypeKind.Delegate => "d",
                _ => "?"
            };
        }
    }
}
