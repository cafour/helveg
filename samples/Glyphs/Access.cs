namespace Helveg.Samples.Glyphs;

public class PublicClass
{
    public int Prop0 { get; set; }
    public int Prop1 { get; set; }
    public int Prop2 { get; set; }
}

internal class InternalClass
{
    public int Prop0 { get; set; }
    public int Prop1 { get; set; }
    public int Prop2 { get; set; }
}

public class ParentClass
{
    protected class ProtectedClass
    {
        public int Prop0 { get; set; }
        public int Prop1 { get; set; }
        public int Prop2 { get; set; }
    }
    
    private class PrivateClass
    {
        public int Prop0 { get; set; }
        public int Prop1 { get; set; }
        public int Prop2 { get; set; }
    }
}
