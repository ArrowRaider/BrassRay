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

        // not sure if this should be made virtual
        /// <summary>
        /// Derived classes must provide the logic that calculates the intersection between a ray and this drawable
        /// </summary>
        /// <returns>Details about the point of intersection, if exists, null otherwise</returns>
        protected abstract Intersection? IntersectCore(Ray ray);

        /// <summary>
        /// Finds the nearest positive-direction intersection between this drawable and a ray, if such an intersection exists
        /// </summary>
        /// <returns>Details about the point of intersection, if exists, null otherwise</returns>
        public Intersection? Intersect(Ray ray)
        {
            var objRay = Ray.Transform(ray, _inverseTransform);
            var o = IntersectCore(objRay);
            if (!o.HasValue) return null;
            var p = Vector3.Transform(o.Value.Position, Transform);
            var n = Vector3.TransformNormal(o.Value.Normal, _inverseTransposeTransform);
            return new Intersection(o.Value.T, p, n, o.Value.Inside, o.Value.Drawable);
        }
    }
}
