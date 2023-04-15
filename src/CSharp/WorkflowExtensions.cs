using Helveg.CSharp.Packages;
using Helveg.CSharp.Projects;
using Helveg.CSharp.Symbols;
using Microsoft.Extensions.Logging;

namespace Helveg.CSharp;

public static class WorkflowExtensions
{
    public static Workflow AddMSBuild(
        this Workflow workflow,
        MSBuildMinerOptions? options = default,
        // TODO: Add a service collection into Workflow.
        ILogger<MSBuildMiner>? logger = default)
    {
        return workflow.AddMiner(new MSBuildMiner(options ?? new(), logger));
    }

    public static Workflow AddNuGet(
        this Workflow workflow,
        NuGetMinerOptions? options = default,
        ILogger<NuGetMiner>? logger = default)
    {
        return workflow.AddMiner(new NuGetMiner(options ?? new(), logger));
    }

    public static Workflow AddRoslyn(
        this Workflow workflow,
        RoslynMinerOptions? options = default,
        ILogger<RoslynMiner>? logger = default)
    {
        return workflow.AddMiner(new RoslynMiner(options ?? new(), logger));
    }
}
