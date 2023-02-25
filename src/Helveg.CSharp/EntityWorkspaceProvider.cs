using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface EntityWorkspaceProvider
{
    Task<EntityWorkspace> GetWorkspace(string path, CancellationToken cancellationToken = default);
}
