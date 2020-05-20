using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg
{
    public static class Vku
    {
        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static extern int helloTriangle();

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int helloMesh(Mesh.Raw mesh);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int createGraphRender(InteropGraph.Raw graph, void** ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int stepGraphRender(void* ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int destroyGraphRender(void* ptr);

        public static unsafe void HelloTriangle()
        {
            var result = helloTriangle();
            if (result != 0)
            {
                throw new InvalidOperationException($"The 'helloTriangle' function returned {result}.");
            }
        }

        public static unsafe int HelloMesh(Mesh mesh)
        {
            fixed (Vector3* vertices = mesh.Vertices)
            fixed (Vector3* colors = mesh.Colors)
            fixed (int* indices = mesh.Indices)
            {
                return helloMesh(new Mesh.Raw
                {
                    Vertices = vertices,
                    Colors = colors,
                    Indices = indices,
                    VertexCount = mesh.Vertices.Length,
                    IndexCount = mesh.Indices.Length
                });
            }
        }

        public static unsafe GraphRender CreateGraphRender(InteropGraph graph)
        {
            var render = new GraphRender
            {
                PositionsPin = GCHandle.Alloc(graph.Positions, GCHandleType.Pinned),
                WeightsPin = GCHandle.Alloc(graph.Weights, GCHandleType.Pinned),
            };
            render.Graph = new InteropGraph.Raw
            {
                Positions = (Vector2*)render.PositionsPin.AddrOfPinnedObject().ToPointer(),
                Weights = (float*)render.WeightsPin.AddrOfPinnedObject().ToPointer(),
                Count = graph.Positions.Length
            };
            var result = createGraphRender(render.Graph, &render.Render);
            if (result != 0)
            {
                render.PositionsPin.Free();
                render.WeightsPin.Free();
                throw new InvalidOperationException($"The 'createGraphRender' function returned {result}.");
            }
            return render;
        }

        public static unsafe bool StepGraphRender(GraphRender render)
        {
            // returns true if stepping should stop
            var result = stepGraphRender(render.Render);
            return result != 0;
        }

        public static unsafe void DestroyGraphRender(GraphRender render)
        {
            int result = destroyGraphRender(render.Render);
            if (result != 0)
            {
                throw new InvalidOperationException($"The 'destroyGraphRender' function returned {result}.");
            }

            render.PositionsPin.Free();
            render.WeightsPin.Free();
        }

    }
}
