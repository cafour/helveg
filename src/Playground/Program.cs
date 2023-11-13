using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helveg.Playground;

public static class Program
{
    public static void Main(string[] args)
    {
        
        var comment = @"
/// <summary>
/// Visitor that converts XML documentation comments to Markdown.
/// </summary>
/// <remarks>
/// Based on a sample from [NuDoq](https://github.com/devlooped/NuDoq).
/// </remarks>";

        var tree = CSharpSyntaxTree.ParseText(comment, CSharpParseOptions.Default);
        var root = tree.GetRoot();
        var trivia = root.DescendantNodes(descendIntoTrivia: true)
            .OfType<StructuredTriviaSyntax>()
            .ToList();
            
        var trivia2 = SyntaxFactory.ParseLeadingTrivia(comment);
        var trivia3 = trivia2.Select(t => t.GetStructure())
            .OfType<StructuredTriviaSyntax>()
            .ToList();

        Console.WriteLine(root.ToString());
    }
}
