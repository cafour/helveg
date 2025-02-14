using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helveg.CSharp.Projects;
using Microsoft.Extensions.Logging;

namespace Helveg.CSharp;

public static class WorkspaceExtensions
{
    public static async Task<ExclusiveEntityHandle<Solution>?> GetSolutionExclusively(
        this Workspace workspace,
        ILogger logger,
        CancellationToken token = default
    )
    {
        var solutions = workspace.Roots.Values.OfType<Solution>().ToArray();
        if (solutions.Length == 0)
        {
            logger.LogError("The workspace contains no Solution therefore no symbols can be mined.");
            return null;
        }
        else if (solutions.Length > 1)
        {
            logger.LogError("The workspace contains multiple solutions but RoslynMiner can only mine one.");
            return null;
        }

        var solution = solutions[0];
        var handle = await workspace.GetRootExclusively<Solution>(solution.Id, token);
        if (handle.Entity is null)
        {
            logger.LogError("The solution has been removed from the workspace while the miner was running.");
            return null;
        }

        return handle;
    }
}
