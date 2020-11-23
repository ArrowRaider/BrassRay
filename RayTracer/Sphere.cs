using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public class Sphere : Drawable
    {
        public float Radius { get; set; }
        public override BoundingBox ObjectBounds =>
            new BoundingBox(Vector3.Zero, Radius * 2.0f, Radius * 2.0f, Radius * 2.0f);

        protected override Intersection? IntersectCore(in Ray ray)
        {
            var a = Vector3.Dot(ray.Direction, ray.Direction);
            var halfB = Vector3.Dot(ray.Position, ray.Direction);
            var c = Vector3.Dot(ray.Position, ray.Position) - Radius * Radius;
            var x = halfB * halfB - a * c;
            if (x < Utils.Epsilon)
                return null;

            var sx = MathF.Sqrt(x);
            var t = (-halfB - sx) / a;
            if (t < Utils.Epsilon)
            {
                t = (-halfB + sx) / a;
                if (t < Utils.Epsilon)
                    return null;
                var p = ray.Position + ray.Direction * t;
                return new Intersection(t, p, -p, p, true, this);
            }
            else
            {
                var p = ray.Position + ray.Direction * t;
                return new Intersection(t, p, p, p, false, this);
            }
        }
    }
}
