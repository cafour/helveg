using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace VkInterop
{
    public class Program
    {
        [DllImport("vki", CallingConvention = CallingConvention.Cdecl)]
        public static extern int helloTriangle();

        [DllImport("vki", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int helloMesh(Mesh.Raw mesh);

        public unsafe static int HelloMesh(Mesh mesh)
        {
            fixed (Vector3* vertices = mesh.Vertices)
            fixed (Vector3* colors = mesh.Colors)
            fixed (uint* indices = mesh.Indices)
            {
                return helloMesh(new Mesh.Raw
                {
                    Vertices = vertices,
                    Colors = colors,
                    Indices = indices,
                    VertexCount = (uint)mesh.Vertices.Length,
                    IndexCount = (uint)mesh.Indices.Length
                });
            }
        }

        public static void Main(string[] args)
        {
            var positions = new[]{
                new Vector3(1, 1, 1),
                new Vector3(1, 1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, -1, 1),
                new Vector3(-1, 1, 1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, -1, -1),
                new Vector3(-1, -1, 1)
            };

            var colors = positions.Select(v => (v + new Vector3(1)) / 2)
                .ToArray();

            var indices = new uint[] {
                0, 1, 3, 3, 1, 2,
                1, 5, 2, 2, 5, 6,
                4, 5, 7, 7, 5, 6,
                4, 0, 7, 7, 0, 3,
                2, 6, 3, 3, 6, 7,
                0, 4, 1, 1, 4, 5
            };

            var mesh = new Mesh(positions, colors, indices);

            var value = HelloMesh(mesh);
            Console.WriteLine($"Hello's return value: {value}");
        }
    }
}
