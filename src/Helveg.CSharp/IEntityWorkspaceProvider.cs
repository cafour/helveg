using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IEntityWorkspaceProvider
{
    Task<EntityWorkspace> GetWorkspace(
        string path,
        EntityWorkspaceAnalysisOptions? options = null,
        CancellationToken cancellationToken = default);
}
