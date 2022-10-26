using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

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

        public Heightmap(Rectangle rectangle)
            : this(rectangle.X, rectangle.X + rectangle.Width, rectangle.Y, rectangle.Y + rectangle.Height)
        {
        }

        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        public int SizeX { get; }
        public int SizeY { get; }

        public float this[Vector2 p]
        {
            get
            {
                return this[p.X, p.Y];
            }
            set
            {
                this[p.X, p.Y] = value;
            }
        }

        public float this[float x, float y]
        {
            get
            {
                return this[(int)MathF.Round(x), (int)MathF.Round(y)];
            }
            set
            {
                this[(int)MathF.Round(x), (int)MathF.Round(y)] = value;
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

        public int CenterX => (MaxX + MinX) / 2;
        public int CenterY => (MaxY + MinY) / 2;

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

        public void Save(string path)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.WriteLine("P2");
            writer.WriteLine($"{SizeX} {SizeY}");
            writer.WriteLine($"{(int)MathF.Round(data.Max())}");
            for(int y = MinY; y < MaxY; ++y)
            {
                for(int x = MinX; x < MaxX; ++x)
                {
                    writer.Write($"{(int)MathF.Round(this[x, y])} ");
                }
                writer.WriteLine();
            }
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
