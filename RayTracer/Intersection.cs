using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Describes the intersection of a ray and a drawable
    /// </summary>
    public readonly struct Intersection : IEquatable<Intersection>, IComparable<Intersection>, IComparable
    {
        public Intersection(float t, Vector3 position, Vector3 normal, Vector3 textureCoordinates, bool inside, Drawable drawable)
        {
            T = t;
            Position = position;
            Normal = Vector3.Normalize(normal);
            TextureCoordinates = textureCoordinates;
            Inside = inside;
            Drawable = drawable;
        }

        /// <summary>
        /// Relative distance from the ray position to the point of intersection in terms of original ray
        /// </summary>
        public float T { get; }

        /// <summary>
        /// The point of intersection
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// The normal at the point of intersection
        /// </summary>
        public Vector3 Normal { get; }

        public Vector3 TextureCoordinates { get; }

        /// <summary>
        /// Whether ray originated inside the drawable or not
        /// </summary>
        public bool Inside { get; }

        /// <summary>
        /// The drawable that intersects with the ray
        /// </summary>
        public Drawable Drawable { get; }

        public bool Equals(Intersection other) => T.Equals(other.T) && Position.Equals(other.Position) &&
                                                  Normal.Equals(other.Normal) &&
                                                  TextureCoordinates.Equals(other.TextureCoordinates) &&
                                                  Inside == other.Inside && Drawable.Equals(other.Drawable);

        public override bool Equals(object obj) => obj is Intersection other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(T, Position, Normal, TextureCoordinates, Inside, Drawable);

        public static bool operator ==(in Intersection left, in Intersection right) => left.Equals(right);

        public static bool operator !=(in Intersection left, in Intersection right) => !left.Equals(right);

        public int CompareTo(Intersection other) => T.CompareTo(other.T);

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            return obj is Intersection other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Intersection)}");
        }

        public static bool operator <(in Intersection left, in Intersection right) => left.CompareTo(right) < 0;

        public static bool operator >(in Intersection left, in Intersection right) => left.CompareTo(right) > 0;

        public static bool operator <=(in Intersection left, in Intersection right) => left.CompareTo(right) <= 0;

        public static bool operator >=(in Intersection left, in Intersection right) => left.CompareTo(right) >= 0;
    }
}
