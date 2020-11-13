using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer
{
    public readonly struct BoundingBox : IEquatable<BoundingBox>
    {
        public BoundingBox(Vector3 position, float width, float height, float depth)
        {
            Position = position;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public Vector3 Position { get; }
        public float Width { get; }
        public float Height { get; }
        public float Depth { get; }

        public static BoundingBox Zero { get; } = new BoundingBox();

        public bool Equals(BoundingBox other) => Position.Equals(other.Position) && Width.Equals(other.Width) &&
                                                 Height.Equals(other.Height) && Depth.Equals(other.Depth);

        public override bool Equals(object obj) => obj is BoundingBox other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Position, Width, Height, Depth);

        public static bool operator ==(in BoundingBox left, in BoundingBox right) => left.Equals(right);

        public static bool operator !=(in BoundingBox left, in BoundingBox right) => !left.Equals(right);

        public static float Intersect(in BoundingBox boundingBox, in Ray ray)
        {
            var d0 = ray.Position - boundingBox.Position;
            var tNear = float.NegativeInfinity;
            var tFar = float.PositiveInfinity;

            var c1 = -boundingBox.Width / 2.0f;
            var c2 = boundingBox.Width / 2.0f;
            if (Utils.ScalarComparer.Compare(ray.Direction.X, 0.0f) == 0)
            {
                if (d0.X < c1 || d0.X > c2)
                    return float.PositiveInfinity;
            }
            else
            {
                var t1 = (c1 - d0.X) / ray.Direction.X;
                var t2 = (c2 - d0.X) / ray.Direction.X;

                if (t1 > t2)
                {
                    var t3 = t1;
                    t1 = t2;
                    t2 = t3;
                }

                if (t1 > tNear) tNear = t1;
                if (t2 < tFar) tFar = t2;
                if (tNear > tFar || tFar < 0.0)
                    return float.PositiveInfinity;
            }

            c1 = -boundingBox.Height / 2.0f;
            c2 = boundingBox.Height / 2.0f;
            if (Utils.ScalarComparer.Compare(ray.Direction.Y, 0.0f) == 0)
            {
                if (d0.Y < c1 || d0.Y > c2)
                    return float.PositiveInfinity;
            }
            else
            {
                var t1 = (c1 - d0.Y) / ray.Direction.Y;
                var t2 = (c2 - d0.Y) / ray.Direction.Y;

                if (t1 > t2)
                {
                    var t3 = t1;
                    t1 = t2;
                    t2 = t3;
                }

                if (t1 > tNear) tNear = t1;
                if (t2 < tFar) tFar = t2;
                if (tNear > tFar || tFar < 0.0)
                    return float.PositiveInfinity;
            }

            c1 = -boundingBox.Depth / 2.0f;
            c2 = boundingBox.Depth / 2.0f;
            if (Utils.ScalarComparer.Compare(ray.Direction.Z, 0.0f) == 0)
            {
                if (d0.Z < c1 || d0.Z > c2)
                    return float.PositiveInfinity;
            }
            else
            {
                var t1 = (c1 - d0.Z) / ray.Direction.Z;
                var t2 = (c2 - d0.Z) / ray.Direction.Z;

                if (t1 > t2)
                {
                    var t3 = t1;
                    t1 = t2;
                    t2 = t3;
                }

                if (t1 > tNear) tNear = t1;
                if (t2 < tFar) tFar = t2;
                if (tNear > tFar || tFar < 0.0)
                    return float.PositiveInfinity;
            }

            return tNear < Utils.Epsilon ? tFar : tNear;
        }

        public static bool Intersect(in BoundingBox boundingBox, Vector3 p)
        {
            var v = p - boundingBox.Position;
            return MathF.Abs(v.X) <= boundingBox.Width / 2.0f &&
                   MathF.Abs(v.Y) <= boundingBox.Height / 2.0f &&
                   MathF.Abs(v.Z) <= boundingBox.Depth / 2.0f;
        }

        public static bool Intersect(in BoundingBox box1, in BoundingBox box2)
        {
            var left1 = box1.Position.X - box1.Width / 2.0f;
            var right1 = box1.Position.X + box1.Width / 2.0f;
            var bottom1 = box1.Position.Y - box1.Height / 2.0f;
            var top1 = box1.Position.Y + box1.Height / 2.0f;
            var back1 = box1.Position.Z - box1.Depth / 2.0f;
            var front1 = box1.Position.Z + box1.Depth / 2.0f;

            var left2 = box2.Position.X - box2.Width / 2.0f;
            var right2 = box2.Position.X + box2.Width / 2.0f;
            var bottom2 = box2.Position.Y - box2.Height / 2.0f;
            var top2 = box2.Position.Y + box2.Height / 2.0f;
            var back2 = box2.Position.Z - box2.Depth / 2.0f;
            var front2 = box2.Position.Z + box2.Depth / 2.0f;

            return left1 <= right2 && right1 >= left2 &&
                   bottom1 <= top2 && top1 >= bottom2 &&
                   back1 <= front2 && front1 >= back2;
        }
    }
}
