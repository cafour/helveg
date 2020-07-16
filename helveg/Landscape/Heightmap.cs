using System;
using System.Collections;
using System.Collections.Generic;

namespace Helveg.Landscape
{
    public class Heightmap : IEnumerable<float>
    {
        private readonly float[] data;
        public Heightmap(int minX, int maxX, int minY, int maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            SizeX = Math.Abs(maxX - minX);
            SizeY = Math.Abs(maxY - minY);
            data = new float[SizeX * SizeY];
        }

        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        public int SizeX { get; }
        public int SizeY { get; }

        public float this[float x, float y]
        {
            get
            {
                return this[(int)x, (int)y];
            }
            set
            {
                this[(int)x, (int)y] = value;
            }
        }

        public float this[int x, int y]
        {
            get
            {
                return data[(y - MinY) * SizeX + (x - MinX)];
            }
            set
            {
                data[(y - MinY) * SizeX + (x - MinX)] = value;
            }
        }

        public bool TryGetValue(int x, int y, out float value)
        {
            if (x >= MinX && x < MaxX && y >= MinY && y < MaxY)
            {
                value = this[x, y];
                return true;
            }

            value = -1f;
            return false;
        }

        public IEnumerator<float> GetEnumerator()
        {
            return ((IEnumerable<float>)data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}
