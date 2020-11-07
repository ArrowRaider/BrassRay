using System.Numerics;

namespace BrassRay.RayTracer
{
    public class InfinitePlane : Drawable
    {
        private Vector3 _normal;
        public Vector3 Position { get; set; }

        public Vector3 Normal
        {
            get => _normal;
            set => _normal = Vector3.Normalize(value);
        }

        protected override Intersection? IntersectCore(Ray ray)
        {
            var n = ray.Inside ? Normal : -Normal;
            var denom = Vector3.Dot(n, ray.Direction);
            if (denom < Utils.Epsilon)
                return null;

            var diff = Position - ray.Position;

            var t = Vector3.Dot(diff, n) / denom;
            var p = ray.Position + ray.Direction * t;
            return new Intersection(t, new Ray(p, -n, ray.Inside), this);
        }
    }
}
