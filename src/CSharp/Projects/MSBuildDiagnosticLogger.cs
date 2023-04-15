using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSB = Microsoft.Build;

namespace Helveg.CSharp.Projects;


// Based on Roslyn's MSBuildDiagnosticLogger.
internal class MSBuildDiagnosticLogger : MSB.Framework.ILogger
{
    private readonly ILogger? logger;
    private MSB.Framework.IEventSource? eventSource;

    public MSBuildDiagnosticLogger(ILogger? logger)
    {
        this.logger = logger;
    }

    public MSB.Framework.LoggerVerbosity Verbosity { get; set; }

    public string? Parameters { get; set; }

    public List<Diagnostic> Diagnostics { get; } = new();

    public void Initialize(MSB.Framework.IEventSource eventSource)
    {
        this.eventSource = eventSource;
        this.eventSource.ErrorRaised += OnErrorRaised;
        this.eventSource.WarningRaised += OnWarningRaised;
    }

    public void Shutdown()
    {
        if (eventSource is not null)
        {
            eventSource.ErrorRaised -= OnErrorRaised;
            eventSource.WarningRaised -= OnWarningRaised;
        }
    }

    private void OnErrorRaised(object sender, MSB.Framework.BuildErrorEventArgs e)
    {
        logger?.LogDebug("msbuild error: {}: {}", e.ProjectFile, e.Message);
        Diagnostics.Add(Diagnostic.Error(e.Code, e.Message ?? string.Empty));
    }

    private void OnWarningRaised(object sender, MSB.Framework.BuildWarningEventArgs e)
    {
        logger?.LogDebug("msbuild warning: {}: {}", e.ProjectFile, e.Message);
        Diagnostics.Add(Diagnostic.Warning(e.Code, e.Message ?? string.Empty));
    }
}
