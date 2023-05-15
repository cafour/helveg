namespace Helveg.Samples.Glyphs;

public class Members
{
    public int Property { get; set; }
    public int Field;
    public int Method(int param) => 0;
    public int this[int index] => 0;
    public event EventHandler Event;
    public delegate int Delegate();
    public record Nested;
}
