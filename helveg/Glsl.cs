using System;
using System.Numerics;

namespace Helveg
{
    public static class Glsl
    {
        public static float Smoothstep(float edge0, float edge1, float x)
        {
            // https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/smoothstep.xhtml
            x = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            return x * x * (3.0f - 2.0f * x);
        }

        public static Vector3 Mix(Vector3 x, Vector3 y, Vector3 a)
        {
            // https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/mix.xhtml
            return Vector3.Cross(x, Vector3.One - a) + Vector3.Cross(y, a);
        }

        public static float Mix(float x, float y, float a)
        {
            return x * (1.0f - a) + y * a;
        }
    }
}
