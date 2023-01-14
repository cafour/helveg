using System.Runtime.InteropServices;

namespace Helveg.Render
{
    public unsafe struct GraphRender
    {
        public GCHandle PositionsPin;
        public GCHandle WeightsPin;
        public Graph.Raw Graph;
        public void *Render;
    }
}
