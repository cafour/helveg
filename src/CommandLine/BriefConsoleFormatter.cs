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
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Helveg;

public class BriefConsoleFormatterOptions : ConsoleFormatterOptions
{
    public bool IncludeStacktraces { get; set; }
}

public class BriefConsoleFormatter : ConsoleFormatter
{
    private readonly IOptionsMonitor<BriefConsoleFormatterOptions> options;

    public BriefConsoleFormatter(IOptionsMonitor<BriefConsoleFormatterOptions> options) : base("brief")
    {
        this.options = options;
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
            LogLevel.Warning => Ansi.Color.Foreground.Yellow,
            LogLevel.Information => Ansi.Color.Foreground.Green,
            LogLevel.Trace => Ansi.Color.Foreground.DarkGray,
            _ => Ansi.Color.Foreground.Default
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

        var sb = new StringBuilder();
        if (options.CurrentValue.TimestampFormat is not null)
        {
            var time = $"[{(options.CurrentValue.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
                .ToString(options.CurrentValue.TimestampFormat)}] ";
            sb.Append(time);
        }

        sb.Append($"{levelColor}{logLevelName}{Ansi.Color.Background.Default}{Ansi.Color.Foreground.Default}: ");

        if (!string.IsNullOrEmpty(category))
        {
            sb.Append($"{Ansi.Color.Foreground.DarkGray}{category}{Ansi.Color.Foreground.Default}: ");
        }

        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message) && logEntry.Exception is not null)
        {
            message = logEntry.Exception.Message;
        }

        sb.Append(message);

        textWriter.WriteLine(sb.ToString());

        if (logEntry.Exception is not null && options.CurrentValue.IncludeStacktraces)
        {
            textWriter.WriteLine($"{Ansi.Color.Foreground.DarkGray}{logEntry.Exception.Demystify()}{Ansi.Color.Foreground.Default}");
        }
    }
}
