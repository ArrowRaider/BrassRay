using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public static class Utils
    {
        public const int DefaultDepth = 30;
        public const float Epsilon = 1.0e-4f;

        public static Vector2 DiscRandom(Random r)
        {
            Vector2 result;
            do
            {
                result = new Vector2((float)r.NextDouble() * 2.0f - 1.0f, (float)r.NextDouble() * 2.0f - 1.0f);
            } while (result.LengthSquared() > 1.0f);
            return result;
        }

        public static Vector3 SphereRandom(Random r)
        {
            Vector3 result;
            do
            {
                result = new Vector3((float)r.NextDouble() * 2.0f - 1.0f, (float)r.NextDouble() * 2.0f - 1.0f,
                    (float)r.NextDouble() * 2.0f - 1.0f);
            } while (result.LengthSquared() > 1.0f);
            return result;
        }

        public static readonly IComparer<float> ScalarComparer = new MyComparer();

        private sealed class MyComparer : Comparer<float>
        {
            public override int Compare([AllowNull] float x, [AllowNull] float y) =>
                MathF.Abs(x - y).CompareTo(Epsilon) <= 0 ? 0 : x.CompareTo(y);
        }
    }
}
