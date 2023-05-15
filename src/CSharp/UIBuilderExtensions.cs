using System;
using System.IO;
using System.Threading.Tasks;
using Helveg.UI;

namespace Helveg.CSharp;

public static class UIBuilderExtensions
{
    public static async Task<UIBuilder> UseCSharp(this UIBuilder sfb)
    {
        var ass = typeof(UIBuilderExtensions).Assembly;
        sfb.AddIconSet(await IconSet.LoadFromAssembly("csharp", ass));
        
        sfb.AddPlugin("helvegCSharp");
        
        // TODO: Move the csharpPlugin to its own project.
        // using var scriptStream = ass.GetManifestResourceStream("helveg-csharp.js");
        // if (scriptStream is null)
        // {
        //     throw new NotSupportedException("The library does not seem to contain its client script. This is likely a bug.");
        // }

        // using var reader = new StreamReader(scriptStream);
        // var script = await reader.ReadToEndAsync();

        // sfb.AddScript("helveg-csharp.js", script);
        return sfb;
    }
}
