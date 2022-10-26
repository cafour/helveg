using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg.Render
{
    public static class Vku
    {
        public const string HelvegWindowTitle = "Helveg";

        public delegate void LogHandler(int level, string message);
        // it's necessary to keep a reference to the logging handlers so that they don't get GC'd
        private static readonly List<LogHandler> logHandlers = new List<LogHandler>();

#pragma warning disable IDE1006
        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool setDebug(bool debug);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool setRayTracing(bool rayTracing);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool setForceCursor(bool forceCursor);


        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool setLogCallback(LogHandler handler);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static extern int helloTriangle();

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int helloMesh(Mesh.Raw mesh);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int createGraphRender(Graph.Raw graph, float scale, void** ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int stepGraphRender(void* ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int destroyGraphRender(void* ptr);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int helloChunk(Chunk.Raw chunk);

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int helloWorld(World.Raw world, string title);
#pragma warning restore IDE1006

        public static void SetDebug(bool debug)
        {
            setDebug(debug);
        }

        public static void SetRayTracing(bool rayTracing)
        {
            setRayTracing(rayTracing);
        }

        public static void SetForceCursor(bool forceCursor)
        {
            setForceCursor(forceCursor);
        }

        public static void SetLogCallback(LogHandler handler)
        {
            logHandlers.Add(handler);
            setLogCallback(handler);
        }

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

        public static unsafe GraphRender CreateGraphRender(Graph graph, float scale = 0.01f)
        {
            var render = new GraphRender
            {
                PositionsPin = GCHandle.Alloc(graph.Positions, GCHandleType.Pinned),
                WeightsPin = GCHandle.Alloc(graph.Weights, GCHandleType.Pinned),
            };
            render.Graph = new Graph.Raw
            {
                Positions = (Vector2*)render.PositionsPin.AddrOfPinnedObject().ToPointer(),
                Weights = (float*)render.WeightsPin.AddrOfPinnedObject().ToPointer(),
                Count = graph.Positions.Length
            };
            var result = createGraphRender(render.Graph, scale, &render.Render);
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

        public static void HelloChunk(Chunk chunk)
        {
            var raw = chunk.GetRaw();
            var result = helloChunk(raw);
            if (result != 0)
            {
                throw new InvalidOperationException($"The 'helloChunk' function returned {result}.");
            }
        }

        public static void HelloWorld(World world)
        {
            var raw = world.GetRaw();
            var result = helloWorld(raw, HelvegWindowTitle);
            if (result != 0)
            {
                throw new InvalidOperationException($"The 'helloWorld' function returned {result}.");
            }
        }
    }
}
