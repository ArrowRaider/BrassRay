using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public abstract class Drawable
    {
        private Matrix4x4 _transform = Matrix4x4.Identity;
        private Matrix4x4 _matrix = Matrix4x4.Identity;
        private Matrix4x4 _inverseMatrix = Matrix4x4.Identity;
        private Matrix4x4 _inverseTransposeMatrix = Matrix4x4.Identity;
        private Vector3 _position = Vector3.Zero;

        public Material Material { get; set; }

        public Matrix4x4 Transform
        {
            get => _transform;
            set
            {
                _transform = value;
                UpdateMatrix();
            }
        }

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateMatrix();
            }
        }

        public Projection TextureProjection { get; set; } = Projection.None;

        public abstract BoundingBox ObjectBounds { get; }

        private void UpdateMatrix()
        {
            _matrix = Matrix4x4.CreateTranslation(Position) * Transform;
            if (!Matrix4x4.Invert(_matrix, out _inverseMatrix))
                throw new InvalidOperationException();
            _inverseTransposeMatrix = Matrix4x4.Transpose(_inverseMatrix);
        }

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
            var objRay = Ray.Transform(ray, _inverseMatrix);
            var oi = IntersectCore(objRay);
            if (!oi.HasValue) return null;
            var wp = Vector3.Transform(oi.Value.Position, _matrix);
            var wn = Vector3.TransformNormal(oi.Value.Normal, _inverseTransposeMatrix);

            Vector3 tc;
            switch (TextureProjection)
            {
                case Projection.None:
                    tc = oi.Value.TextureCoordinates;
                    break;
                case Projection.PlaneXy:
                    tc = new Vector3(oi.Value.Position.X, oi.Value.Position.Y, 0.0f);
                    break;
                case Projection.Sphere:
                    {
                        var r = oi.Value.Position.Length();
                        var theta = oi.Value.Position.Z != 0.0f || oi.Value.Position.X != 0.0f
                            ? MathF.Atan2(oi.Value.Position.Z, oi.Value.Position.X)
                            : 0.0f;
                        var phi = r > 0.0 ? MathF.Acos(oi.Value.Position.Y / r) : 0.0f;
                        var u = (theta + MathF.PI) / (2.0f * MathF.PI);
                        var v = phi / MathF.PI;
                        tc = new Vector3(u, v, 0.1f);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

            return new Intersection(oi.Value.T, wp, wn, tc, oi.Value.Inside, oi.Value.Drawable);
        }

        public float IntersectBounds(in Ray ray)
        {
            var objRay = Ray.Transform(ray, _inverseMatrix);
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
                points[i] = Vector3.Transform(points[i], _matrix);
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
