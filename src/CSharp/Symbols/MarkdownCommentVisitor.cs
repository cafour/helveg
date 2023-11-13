using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Helveg.CSharp;

/// <summary>
/// Visitor that converts XML documentation comments to Markdown.
/// </summary>
internal class MarkdownCommentVisitor
{
    public StringBuilder Output { get; } = new StringBuilder();

    public static string? ToMarkdown(string comment)
    {
        var node = XElement.Parse(comment);
        if (node is null)
        {
            return null;
        }

        var visitor = new MarkdownCommentVisitor();
        visitor.VisitElement(node);
        return visitor.Output.ToString();
    }

    public void Visit(XNode node)
    {
        switch (node)
        {
            case XElement e:
                VisitElement(e);
                break;
            case XText t:
                VisitText(t);
                break;
        }
    }

    private void BaseVisit(XNode node)
    {
    }

    private void VisitChildren(XContainer container)
    {
        foreach (var child in container.Nodes())
        {
            Visit(child);
        }
    }

    public void VisitElement(XElement node)
    {
        switch (node.Name.LocalName.ToLower())
        {
            case "summary":
                VisitSummary(node);
                break;
            case "remarks":
                VisitRemarks(node);
                break;
            case "code":
                VisitCode(node);
                break;
            case "see":
            case "seealso":
                VisitSee(node);
                break;
            default:
                VisitChildren(node);
                BaseVisit(node);
                break;
        }
    }

    public void VisitSummary(XElement summary)
    {
        WriteLine("**Summary**");
        WriteLine();
        VisitChildren(summary);
        BaseVisit(summary);
        WriteLine();
    }

    public void VisitRemarks(XElement remarks)
    {
        WriteLine("**Remarks**");
        WriteLine();
        VisitChildren(remarks);
        BaseVisit(remarks);
        WriteLine();
    }

    public void VisitExample(XElement example)
    {
        WriteLine("**Example**");
        WriteLine();
        VisitChildren(example);
        BaseVisit(example);
        WriteLine();
    }

    public void VisitCode(XElement code)
    {
        WriteLine();
        WriteLine("```");
        WriteLine(code.ToString());
        WriteLine("```");
        WriteLine();
    }

    public void VisitText(XText text)
    {
        Write(text.ToString().Trim());
    }

    public void VisitSee(XElement see)
    {
        var href = see.Attribute("href");
        if (href is not null)
        {
            var link = NormalizeLink(href.ToString());
            Write($" [{link[2..]}]({link}) ");
            return;
        }

        var cref = see.Attribute("cref");
        if (cref is not null)
        {
            // Wrap inline code in ` according to Markdown syntax.
            Write(" `");
            Write(cref.ToString());
            Write("` ");
            return;
        }
    }

    private string NormalizeLink(string cref)
    {
        return cref.Replace(":", "-").Replace("(", "-").Replace(")", "");
    }

    private void Write(string value)
    {
        Output.Append(value);
    }

    private void WriteLine(string? line = null)
    {
        Output.Append(line);
        // Force unix line endings
        Output.Append("\n");
    }
}
