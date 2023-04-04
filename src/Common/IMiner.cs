using System.Threading;
using System.Threading.Tasks;

namespace Helveg;

public interface IMiner
{
    Task Mine(Workspace workspace, CancellationToken cancellationToken = default);
}
