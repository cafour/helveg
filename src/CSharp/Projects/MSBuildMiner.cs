using System;
using System.Threading.Tasks;

namespace Helveg.CSharp.Projects;

public class MSBuildMiner : IMiner
{
    private readonly MSBuildMinerOptions options;

    public MSBuildMiner(MSBuildMinerOptions options)
    {
        this.options = options;
    }

    public Task Mine(Workspace workspace)
    {
        throw new NotImplementedException();
    }
}
