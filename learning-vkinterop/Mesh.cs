using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace VkInterop
{
    public struct Mesh
    {
        public Mesh(Vector3[] vertices, Vector3[] colors, uint[] indices)
        {
            if (vertices.Length != colors.Length || colors.Length != indices.Length)
            {
                throw new ArgumentException("The vertex, color, and index arrays must have the same length.");
            }

            this.vertices = vertices;
            this.colors = colors;
            this.indices = indices;
            this.count = (uint)vertices.Length;
        }

        private Vector3[] vertices;
        private Vector3[] colors;
        private uint[] indices;
        private uint count;

        public Vector3[] Vertices => vertices;

        public Vector3[] Colors => colors;

        public uint[] Indices => indices;
    }
}
