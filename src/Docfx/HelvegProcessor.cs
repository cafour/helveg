using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
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
        var browsingContext = BrowsingContext.New(Configuration.Default);
        var parser = browsingContext.GetService<IHtmlParser>()
            ?? throw new InvalidOperationException("The HTML parser is missing.");

        foreach (var file in manifest.Files)
        {
            foreach (var output in file.Output)
            {
                if (output.Key != ".html")
                {
                    continue;
                }

                IHtmlDocument document;

                var path = Path.Combine(outputFolder, output.Value.RelativePath);

                using (var stream = File.OpenRead(path))
                {
                    document = parser.ParseDocument(stream);
                }

                var h1 = document.QuerySelector("h1");
                if (h1 is null)
                {
                    Console.WriteLine($"${output.Value.RelativePath}: There is no h1 element to append a Helveg link after.");
                    continue;
                }

                var helvegLink = document.CreateElement("a");
                helvegLink.TextContent = "Show in Helveg";
                var searchTerm = Path.GetFileNameWithoutExtension(output.Value.RelativePath);
                helvegLink.SetAttribute("href", $"/helveg.html?q={searchTerm}");
                h1.After(helvegLink);

                File.WriteAllText(path, document.ToHtml());
            }
        }
        return manifest;
    }
}
