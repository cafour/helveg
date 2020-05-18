using System.Runtime.InteropServices;

namespace Helveg
{
    public unsafe struct GraphRender
    {
        public GCHandle PositionsPin;
        public GCHandle WeightsPin;
        public InteropGraph.Raw Graph;
        public void *Render;
    }
}
