using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Helveg.Visualization;

namespace Helveg.UI;

public static class UIConst
{
    public const string ExplorerCssResourceName = "helveg-explorer.css";
    public const string DiagramJsResourceName = "helveg-diagram.js";
    public const string ExplorerJsResourceName = "helveg-explorer.js";
    public const string VscodeIconSetResourceName = "helveg-icons-vscode.json";
    public const string HelvegIconSetResourceName = "helveg-icons-helveg.json";
    public const string PizzaIconSetResourceName = "helveg-icons-pizza.json";

    public static readonly Lazy<string> ExplorerCss = new(() => GetBaseResource(ExplorerCssResourceName));
    public static readonly Lazy<string> DiagramJs = new(() => GetBaseResource(DiagramJsResourceName));
    public static readonly Lazy<string> ExplorerJs = new(() => GetBaseResource(ExplorerJsResourceName));
    public static readonly Lazy<string> VsIconSet = new(() => GetBaseResource(VscodeIconSetResourceName));
    public static readonly Lazy<string> NugetIconSet = new(() => GetBaseResource(HelvegIconSetResourceName));
    public static readonly Lazy<string> PizzaIconSet = new(() => GetBaseResource(PizzaIconSetResourceName));

    private static string GetBaseResource(string name)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Stream? stream = null;
        if (dir is not null)
        {
            var namePath = Path.Combine(dir, name);
            if (File.Exists(namePath))
            {
                stream = File.OpenRead(namePath);
            }
        }

        if (stream is null)
        {
            stream = typeof(UIBuilder).Assembly.GetManifestResourceStream(name);
        }

        if (stream is null)
        {
            throw new ArgumentException($"Could not find resource '{name}'.");
        }

        using (stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
