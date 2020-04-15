using System;
using System.Numerics;

namespace Helveg
{
    public struct Mesh
    {
        public Mesh(Vector3[] vertices, Vector3[] colors, int[] indices)
        {
            if (vertices.Length != colors.Length)
            {
                throw new ArgumentException("The vertex and color arrays must have the same length.");
            }
            Vertices = vertices;
            Colors = colors;
            Indices = indices;
        }

        public Vector3[] Vertices { get; }

        public Vector3[] Colors { get; }

        public int[] Indices { get; }

        public unsafe struct Raw
        {
            public Vector3* Vertices;
            public Vector3* Colors;
            public int* Indices;
            public int VertexCount;
            public int IndexCount;
        }
    }
}
