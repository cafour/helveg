namespace Helveg;

public record Comment(
    CommentFormat Format,
    string Content
);

public enum CommentFormat
{
    Plain,
    Markdown
}
