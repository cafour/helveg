using System.Threading.Tasks;

namespace Helveg;

public interface IMiner
{
    Task Mine(Workspace workspace);
}
