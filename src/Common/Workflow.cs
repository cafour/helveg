using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Helveg;

public class Workflow : IDisposable
{
    private readonly List<IMiner> miners = new();

    public static Workflow CreateDefault()
    {
        return new();
    }

    public Workflow AddMiner(IMiner miner)
    {
        miners.Add(miner);
        return this;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var miner in miners)
        {
            if (miner is IDisposable disposableMiner)
            {
                disposableMiner.Dispose();
            }
        }
    }

    public async Task<Workspace> Run(DataSource source, CancellationToken cancellationToken = default)
    {
        // TODO: topological sort

        var workspace = new Workspace
        {
            Source = source
        };

        foreach (var miner in miners)
        {
            await miner.Mine(workspace, cancellationToken);
        }

        return workspace;
    }
}
