using System;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public class NuGetMiner : IMiner
{
    private readonly NuGetMinerOptions options;

    public NuGetMiner(NuGetMinerOptions options)
    {
        this.options = options;
    }
    
    public Task Mine(Workspace workspace)
    {
        throw new NotImplementedException();
    }
}
