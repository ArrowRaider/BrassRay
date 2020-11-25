﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public static class Utils
    {
        public const int DefaultDepth = 30;
        public const float Epsilon = 1.0e-4f;

        public static Vector2 DiscRandom(Sobol.Sequence sequence, int dimension)
        {
            Vector2 result;
            do
            {
                result = new Vector2(sequence.Get(dimension) * 2.0f - 1.0f,
                    sequence.Get(dimension + 1) * 2.0f - 1.0f);
            } while (result.LengthSquared() > 1.0f);
            return result;
        }

        public static Vector3 SphereRandom(Sobol.Sequence sequence, int dimension)
        {
            Vector3 result;
            do
            {
                result = new Vector3(sequence.Get(dimension) * 2.0f - 1.0f,
                    sequence.Get(dimension + 1) * 2.0f - 1.0f,
                    sequence.Get(dimension + 2) * 2.0f - 1.0f);
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
