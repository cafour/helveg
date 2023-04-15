namespace Helveg;

public record Diagnostic(
    string Id,
    string Message,
    DiagnosticSeverity Severity
)
{
    public static Diagnostic Error(string id, string message)
    {
        return new Diagnostic(id, message, DiagnosticSeverity.Error);
    }

    public static Diagnostic Warning(string id, string message)
    {
        return new Diagnostic(id, message, DiagnosticSeverity.Warning);
    }

    public static Diagnostic Info(string id, string message)
    {
        return new Diagnostic(id, message, DiagnosticSeverity.Info);
    }
}
