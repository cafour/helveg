using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Helveg.Render
{
    public struct Point3 : IEquatable<Point3>
    {
        public int X;
        public int Y;
        public int Z;

        public static readonly Point3 Zero = new Point3(0, 0, 0);

        public Point3(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Point3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(float x, float y, float z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        public Point3(Vector3 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
            Z = (int)vector.Z;
        }

        public static Point3 Sign(Point3 point)
        {
            return new Point3(Math.Sign(point.X), Math.Sign(point.Y), Math.Sign(point.Z));
        }

        public static Point3 Round(Vector3 vector)
        {
            return new Point3(MathF.Round(vector.X), MathF.Round(vector.Y), MathF.Round(vector.Z));
        }

        public static int Max(Point3 point)
        {
            return Math.Max(point.X, Math.Max(point.Y, point.Z));
        }

        public static int Min(Point3 point)
        {
            return Math.Min(point.X, Math.Min(point.Y, point.Z));
        }

        public static Point3 Abs(Point3 point)
        {
            return new Point3(Math.Abs(point.X), Math.Abs(point.Y), Math.Abs(point.Z));
        }

        public static bool operator ==(Point3 left, Point3 right)
            => left.Equals(right);

        public static bool operator !=(Point3 left, Point3 right)
            => !left.Equals(right);

        public static explicit operator Vector3(Point3 coord)
            => new Vector3(coord.X, coord.Y, coord.Z);

        public static explicit operator Point3(Vector3 vector)
            => new Point3(vector);

        public static Point3 operator %(Point3 coord, int modulo)
            => new Point3(coord.X % modulo, coord.Y % modulo, coord.Z % modulo);

        public static Point3 operator +(Point3 left, Point3 right)
            => new Point3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        public static Point3 operator -(Point3 coord)
            => new Point3(-coord.X, -coord.Y, -coord.Z);

        public static Point3 operator -(Point3 left, Point3 right)
            => new Point3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        public static Point3 operator *(Point3 left, Point3 right)
            => new Point3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

        public static Point3 operator *(Point3 coord, int factor)
            => new Point3(coord.X * factor, coord.Y * factor, coord.Z * factor);

        public static Point3 operator *(int factor, Point3 coord)
            => coord * factor;

        public static Point3 operator /(Point3 left, Point3 right)
            => new Point3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

        public static Point3 operator /(Point3 coord, int divisor)
            => new Point3(coord.X / divisor, coord.Y / divisor, coord.Z / divisor);

        public static Point3 operator /(int divisor, Point3 coord)
            => coord / divisor;

        public override bool Equals(object? obj)
        {
            if (obj is Point3 coord)
            {
                return Equals(coord);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public bool Equals([AllowNull] Point3 coord)
        {
            return X == coord.X &&
                   Y == coord.Y &&
                   Z == coord.Z;
        }

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}
