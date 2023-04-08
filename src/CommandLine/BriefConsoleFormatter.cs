using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine.Rendering;

namespace Helveg;

public class BriefConsoleFormatter : ConsoleFormatter
{
    public BriefConsoleFormatter() : base("brief")
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var category = logEntry.Category[(logEntry.Category.LastIndexOf('.') + 1)..];
        var levelColor = logEntry.LogLevel switch
        {
            LogLevel.Critical => Ansi.Color.Background.Red,
            LogLevel.Error => Ansi.Color.Foreground.Red,
            LogLevel.Information => Ansi.Color.Foreground.Green,
            LogLevel.Trace => Ansi.Color.Foreground.DarkGray,
            _ => string.Empty
        };

        var logLevelName = logEntry.LogLevel switch
        {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "dbg",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "error",
            LogLevel.Critical => "crit",
            _ => ""
        };

        var time = DateTimeOffset.Now.ToString("HH:mm:ss.fff");
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        textWriter.WriteLine(
            $"[{time}] {levelColor}{logLevelName}{Ansi.Color.Background.Default}{Ansi.Color.Foreground.Default}: " +
            $"{Ansi.Color.Foreground.DarkGray}{category}{Ansi.Color.Foreground.Default}: {message}");
    }
}
