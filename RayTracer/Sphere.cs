using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public class Sphere : Drawable
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }

        protected override Intersection? IntersectCore(Ray ray)
        {
            var diff = ray.Position - Position;
            var a = Vector3.Dot(ray.Direction, ray.Direction);
            var halfB = Vector3.Dot(diff, ray.Direction);
            var c = Vector3.Dot(diff, diff) - Radius * Radius;
            var x = halfB * halfB - a * c;
            if (x < 0.0f)
                return null;
            if (ray.Inside)
            {
                var t = (-halfB + MathF.Sqrt(x)) / a;
                var n0 = ray.Position + ray.Direction * t;
                return new Intersection(t, new Ray(n0, Position - n0, ray.Inside), this);
            }
            else
            {
                var t = (-halfB - MathF.Sqrt(x)) / a;
                var n0 = ray.Position + ray.Direction * t;
                return new Intersection(t, new Ray(n0, n0 - Position, ray.Inside), this);
            }
        }
    }
}
