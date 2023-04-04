using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public class MSBuildMiner : IMiner
{
    private readonly MSBuildMinerOptions options;

    public MSBuildMiner(MSBuildMinerOptions options)
    {
        this.options = options;
    }

    public Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
