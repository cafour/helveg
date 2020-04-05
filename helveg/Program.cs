using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Helveg
{
    public class Program
    {
        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
        public static extern int helloTriangle();

        [DllImport("vku", CallingConvention = CallingConvention.Cdecl)]
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
            var sentence = Spruce.Rewrite("TC", 7, 6, 3, 4, 2);
            Console.WriteLine(sentence);
            Environment.Exit(0);

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
                3, 2, 0, 0, 2, 1,
                2, 6, 1, 1, 6, 5,
                6, 7, 5, 5, 7, 4,
                7, 3, 4, 4, 3, 0,
                0, 1, 4, 4, 1, 5,
                2, 3, 6, 6, 3, 7
            };

            var mesh = new Mesh(positions, colors, indices);

            var value = HelloMesh(mesh);
            Console.WriteLine($"Hello's return value: {value}");
        }
    }
}
