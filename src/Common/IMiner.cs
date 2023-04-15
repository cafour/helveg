using System.Threading;
using System.Threading.Tasks;

namespace Helveg;

public interface IMiner
{
    MinerOptions Options { get; }

    Task Mine(Workspace workspace, CancellationToken cancellationToken = default);
}
