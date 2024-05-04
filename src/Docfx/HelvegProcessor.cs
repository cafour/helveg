using System.Collections.Immutable;
using System.Composition;
using Docfx.Plugins;

namespace Helveg.Docfx;

[Export(nameof(HelvegProcessor), typeof(IPostProcessor))]
public class HelvegProcessor : IPostProcessor
{
    public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
    {
        return metadata;
    }

    public Manifest Process(Manifest manifest, string outputFolder)
    {
        foreach(var file in manifest.Files)
        {
            foreach(var output in file.Output)
            {
            }
        }
        return manifest;
    }
}
