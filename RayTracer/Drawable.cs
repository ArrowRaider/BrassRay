using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public abstract class Drawable
    {
        private Matrix4x4 _transform = Matrix4x4.Identity;
        private Matrix4x4 _inverseTransform = Matrix4x4.Identity;
        private Matrix4x4 _inverseTransposeTransform = Matrix4x4.Identity;

        public Material Material { get; set; }

        public Matrix4x4 Transform
        {
            get => _transform;
            set
            {
                _transform = value;
                if (!Matrix4x4.Invert(_transform, out _inverseTransform))
                    throw new InvalidOperationException();
                _inverseTransposeTransform = Matrix4x4.Transpose(_inverseTransform);
            }
        }

        public abstract BoundingBox ObjectBounds { get; }

        // not sure if this should be made virtual
        /// <summary>
        /// Derived classes must provide the logic that calculates the intersection between a ray and this drawable
        /// </summary>
        /// <returns>Details about the point of intersection, if exists, null otherwise</returns>
        protected abstract Intersection? IntersectCore(in Ray ray);

        /// <summary>
        /// Finds the nearest positive-direction intersection between this drawable and a ray, if such an intersection exists
        /// </summary>
        /// <returns>Details about the point of intersection, if exists, null otherwise</returns>
        public Intersection? Intersect(in Ray ray)
        {
            var objRay = Ray.Transform(ray, _inverseTransform);
            var o = IntersectCore(objRay);
            if (!o.HasValue) return null;
            var p = Vector3.Transform(o.Value.Position, Transform);
            var n = Vector3.TransformNormal(o.Value.Normal, _inverseTransposeTransform);
            return new Intersection(o.Value.T, p, n, o.Value.Inside, o.Value.Drawable);
        }

        public float IntersectBounds(in Ray ray)
        {
            var objRay = Ray.Transform(ray, _inverseTransform);
            return BoundingBox.Intersect(ObjectBounds, objRay);
        }

        public BoundingBox GetBounds()
        {
            if (ObjectBounds == BoundingBox.Zero) return BoundingBox.Zero;

            Span<Vector3> points = stackalloc Vector3[8];
            points[0] = new Vector3(-ObjectBounds.Width / 2.0f, -ObjectBounds.Height / 2.0f,
                -ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[1] = new Vector3(-ObjectBounds.Width / 2.0f, -ObjectBounds.Height / 2.0f,
                ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[2] = new Vector3(-ObjectBounds.Width / 2.0f, ObjectBounds.Height / 2.0f,
                -ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[3] = new Vector3(-ObjectBounds.Width / 2.0f, ObjectBounds.Height / 2.0f,
                ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[4] = new Vector3(ObjectBounds.Width / 2.0f, -ObjectBounds.Height / 2.0f,
                -ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[5] = new Vector3(ObjectBounds.Width / 2.0f, -ObjectBounds.Height / 2.0f,
                ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[6] = new Vector3(ObjectBounds.Width / 2.0f, ObjectBounds.Height / 2.0f,
                -ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            points[7] = new Vector3(ObjectBounds.Width / 2.0f, ObjectBounds.Height / 2.0f,
                ObjectBounds.Depth / 2.0f) + ObjectBounds.Position;
            for (var i = 0; i < 8; i++)
            {
                points[i] = Vector3.Transform(points[i], Transform);
            }

            var left = points[0].X;
            var bottom = points[0].Y;
            var back = points[0].Z;
            var right = left;
            var top = bottom;
            var front = back;
            for (var i = 1; i < 8; i++)
            {
                left = MathF.Min(points[i].X, left);
                bottom = MathF.Min(points[i].Y, bottom);
                back = MathF.Min(points[i].Z, back);
                right = MathF.Max(points[i].X, right);
                top = MathF.Max(points[i].Y, top);
                front = MathF.Max(points[i].Z, front);
            }

            var position = new Vector3((right + left) / 2.0f, (top + bottom) / 2.0f, (front + back) / 2.0f);
            return new BoundingBox(position, right - left, top - bottom, front - back);
        }
    }
}
