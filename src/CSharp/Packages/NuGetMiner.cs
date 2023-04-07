using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp.Packages;

public class NuGetMiner : IMiner
{
    private readonly ILogger<NuGetMiner> logger;

    public NuGetMinerOptions Options { get; }

    MinerOptions IMiner.Options => Options;

    public NuGetMiner(NuGetMinerOptions options, ILogger<NuGetMiner>? logger = null)
    {
        Options = options;
        this.logger = logger ?? NullLoggerFactory.Instance.CreateLogger<NuGetMiner>();
    }

    public Task Mine(Workspace workspace, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
