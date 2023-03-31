namespace Helveg;

public record Diagnostic(
    string Id,
    string Message,
    DiagnosticSeverity Severity
);
