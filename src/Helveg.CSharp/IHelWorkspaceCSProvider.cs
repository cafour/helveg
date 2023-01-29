using System.Threading;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public interface IHelWorkspaceCSProvider
{
    Task<HelWorkspaceCS> GetWorkspace(string path, CancellationToken cancellationToken = default);
}
