using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public class Cylinder : Drawable
    {
        public override BoundingBox ObjectBounds => new(Vector3.Zero, 2 * MathF.Max(Radius1, Radius2), Height,
            2 * MathF.Max(Radius1, Radius2));
        public float Radius1 { get; set; }
        public float Radius2 { get; set; }
        public float Height { get; set; }
        
        protected override Intersection? IntersectCore(in Ray ray)
        {
            float a, halfB, c;

            float normalY = 0;
            if (Utils.ScalarComparer.Compare(Radius1, Radius2) != 0)
            {
                var scale = (Radius1 - Radius2) / Height;
                var shiftY = Height / 2 - Radius1 / scale;
                normalY = -scale * shiftY / shiftY;
                // Coefficients of quadratic equation
                a = ray.Direction.X * ray.Direction.X + ray.Direction.Z * ray.Direction.Z - scale * scale * (ray.Direction.Y * ray.Direction.Y);
                halfB = ray.Direction.X * ray.Position.X + ray.Direction.Z * ray.Position.Z - scale * scale * (ray.Direction.Y * ray.Position.Y - ray.Direction.Y * shiftY);
                c = ray.Position.X * ray.Position.X + ray.Position.Z * ray.Position.Z - scale * scale * (ray.Position.Y * ray.Position.Y - 2 * ray.Position.Y * shiftY + shiftY * shiftY);
            }
            else // Special case of uniform cylinder
            {
                a = ray.Direction.X * ray.Direction.X + ray.Direction.Z * ray.Direction.Z;
                halfB = ray.Direction.X * ray.Position.X + ray.Direction.Z * ray.Position.Z;
                c = ray.Position.X * ray.Position.X + ray.Position.Z * ray.Position.Z - Radius1 * Radius1;
            }

            var x = halfB * halfB - a * c;
            if (x < Utils.Epsilon) return null;
            var sx = MathF.Sqrt(x);
            var t = (-halfB - sx) / a;
            var inside = false;

            if (t < Utils.Epsilon)
            {
                inside = true;
                t = (-halfB + sx) / a;
                if (t < Utils.Epsilon)
                    return null;
            }

            var p = ray.Position + ray.Direction * t;
            if (Math.Abs(p.Y) <= Height / 2.0)
            {
                var normal = new Vector3(p.X, 0, p.Z);
                if (Utils.ScalarComparer.Compare(normalY, 0) != 0)
                {
                    normal = Vector3.Normalize(normal);
                    normal.X *= 1.0f / MathF.Abs(normalY);
                    normal.Y = normalY;
                    normal.Z *= 1.0f / MathF.Abs(normalY);
                }
                
                if (inside)
                    normal = -normal;

                return new Intersection(t, p, normal, p, inside, this);
            }
            // it may have intersected an endcap

            // top cap
            if (Radius1 > Utils.Epsilon && (!inside && ray.Direction.Y < Utils.Epsilon || inside && ray.Direction.Y > Utils.Epsilon))
            {
                t = (Height / 2.0f - ray.Position.Y) / ray.Direction.Y;
                if (t < Utils.Epsilon) return null;
                p = ray.Position + ray.Direction * t;
                if (p.X * p.X + p.Z * p.Z >= Radius1 * Radius1) return null;
                var normal = new Vector3(0, 1, 0);
                if (inside)
                    normal = -normal;
                return new Intersection(t, p, normal, p, inside, this);
            }

            // bottom cap
            if (Radius2 > Utils.Epsilon && (!inside && ray.Direction.Y > Utils.Epsilon || inside && ray.Direction.Y < Utils.Epsilon))
            {
                t = (-Height / 2.0f - ray.Position.Y) / ray.Direction.Y;
                if (t < Utils.Epsilon) return null;
                p = ray.Position + ray.Direction * t;
                if (p.X * p.X + p.Z * p.Z >= Radius2 * Radius2) return null;
                var normal = new Vector3(0, -1, 0);
                if (inside)
                    normal = -normal;
                return new Intersection(t, p, normal, p, inside, this);
            }

            return null;
        }
    }
}
