using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public class NuGetMiner : IMiner
{
    public NuGetMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public NuGetMiner(NuGetMinerOptions options)
    {
        Options = options;
    }

    public Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
