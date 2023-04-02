using Helveg.CSharp.Packages;
using Helveg.CSharp.Projects;
using Helveg.CSharp.Symbols;

namespace Helveg.CSharp;

public static class WorkflowExtensions
{
    public static Workflow AddMSBuild(
        this Workflow workflow,
        MSBuildMinerOptions? options = default)
    {
        return workflow.AddMiner(new MSBuildMiner(options ?? new()));
    }

    public static Workflow AddNuGet(
        this Workflow workflow,
        NuGetMinerOptions? options = default)
    {
        return workflow.AddMiner(new NuGetMiner(options ?? new()));
    }

    public static Workflow AddRoslyn(
        this Workflow workflow,
        RoslynMinerOptions? options = default)
    {
        return workflow.AddMiner(new RoslynMiner(options ?? new()));
    }
}
