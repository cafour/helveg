using System;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public class RoslynMiner : IMiner
{
    private readonly RoslynMinerOptions options;

    public RoslynMiner(RoslynMinerOptions options)
    {
        this.options = options;
    }
    
    public Task Mine(Workspace workspace)
    {
        throw new NotImplementedException();
    }
}
