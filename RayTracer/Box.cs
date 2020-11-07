using System.Numerics;

namespace BrassRay.RayTracer
{
    public class Box : Drawable
    {
        public Box(Vector3 position, float width, float height, float depth)
        {
            Position = position;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public Vector3 Position { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Depth { get; set; }

        protected override Intersection? IntersectCore(Ray ray)
        {
            var d0 = ray.Position - Position;
            var tNear = -float.MaxValue;
            var tFar = float.MaxValue;

            var normal = Vector3.Zero;

            var c1 = -Width / 2.0f;
            var c2 = Width / 2.0f;
            if (Utils.ScalarComparer.Compare(ray.Direction.X, 0.0f) == 0)
            {
                if (d0.X < c1 || d0.X > c2)
                    return null;
            }
            else
            {
                var t1 = (c1 - d0.X) / ray.Direction.X;
                var t2 = (c2 - d0.X) / ray.Direction.X;

                var n = -1.0f;
                if (t1 > t2)
                {
                    var t3 = t1;
                    t1 = t2;
                    t2 = t3;
                    n = -n;
                }

                if (t1 > tNear)
                {
                    tNear = t1;
                    if (!ray.Inside)
                        normal = new Vector3(n, 0.0f, 0.0f);
                }
                if (t2 < tFar)
                {
                    tFar = t2;
                    if (ray.Inside)
                        normal = new Vector3(n, 0.0f, 0.0f);
                }
                if (tNear > tFar || tFar < 0.0)
                    return null;
            }

            c1 = -Height / 2.0f;
            c2 = Height / 2.0f;
            if (Utils.ScalarComparer.Compare(ray.Direction.Y, 0.0f) == 0)
            {
                if (d0.Y < c1 || d0.Y > c2)
                    return null;
            }
            else
            {
                var t1 = (c1 - d0.Y) / ray.Direction.Y;
                var t2 = (c2 - d0.Y) / ray.Direction.Y;

                var n = -1.0f;
                if (t1 > t2)
                {
                    var t3 = t1;
                    t1 = t2;
                    t2 = t3;
                    n = -n;
                }

                if (t1 > tNear)
                {
                    tNear = t1;
                    if (!ray.Inside)
                        normal = new Vector3(0.0f, n, 0.0f);
                }
                if (t2 < tFar)
                {
                    tFar = t2;
                    if (ray.Inside)
                        normal = new Vector3(0.0f, n, 0.0f);
                }
                if (tNear > tFar || tFar < 0.0)
                    return null;
            }

            c1 = -Depth / 2.0f;
            c2 = Depth / 2.0f;
            if (Utils.ScalarComparer.Compare(ray.Direction.Z, 0.0f) == 0)
            {
                if (d0.Z < c1 || d0.Z > c2)
                    return null;
            }
            else
            {
                var t1 = (c1 - d0.Z) / ray.Direction.Z;
                var t2 = (c2 - d0.Z) / ray.Direction.Z;

                var n = -1.0f;
                if (t1 > t2)
                {
                    var t3 = t1;
                    t1 = t2;
                    t2 = t3;
                    n = -n;
                }

                if (t1 > tNear)
                {
                    tNear = t1;
                    if (!ray.Inside)
                        normal = new Vector3(0.0f, 0.0f, n);
                }
                if (t2 < tFar)
                {
                    tFar = t2;
                    if (ray.Inside)
                        normal = new Vector3(0.0f, 0.0f, n);
                }
                if (tNear > tFar || tFar < 0.0)
                    return null;
            }

            var t = ray.Inside ? tFar : tNear;
            var n0 = ray.Position + t * ray.Direction;
            return new Intersection(t, new Ray(n0, normal, ray.Inside), this);
        }
    }
}
