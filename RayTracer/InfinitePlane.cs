using System.Numerics;

namespace BrassRay.RayTracer
{
    public class InfinitePlane : Drawable
    {
        private Vector3 _normal;
        public Vector3 Position { get; set; }

        public override BoundingBox ObjectBounds => BoundingBox.Zero;

        public Vector3 Normal
        {
            get => _normal;
            set => _normal = Vector3.Normalize(value);
        }

        protected override Intersection? IntersectCore(in Ray ray)
        {
            var inside = false;
            var n = Normal;
            var denom = Vector3.Dot(n, ray.Direction);
            var compare = Utils.ScalarComparer.Compare(denom, 0.0f);
            switch (compare)
            {
                case 0:
                    return null;
                case < 0:
                    n = -Normal;
                    denom = -denom;
                    inside = true;
                    break;
            }

            var diff = Position - ray.Position;
            var t = Vector3.Dot(diff, n) / denom;
            if (t < Utils.Epsilon)
                return null;
            var p = ray.Position + ray.Direction * t;
            return new Intersection(t, p, -n, inside, this);
        }
    }
}
