using System.Threading.Tasks;
using Helveg.UI;

namespace Helveg.CSharp;

public static class SingleFileBuilderExtensions
{
    public static async Task<SingleFileBuilder> UseCSharp(this SingleFileBuilder sfb)
    {
        sfb.AddIconSet(await IconSet.LoadFromAssembly("csharp", typeof(SingleFileBuilderExtensions).Assembly));
        return sfb;
    }
}
