using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Represents an infinite ray described by a starting point (Position) and unit vector direction
    /// </summary>
    public readonly struct Ray : IEquatable<Ray>
    {
        /// <summary>
        /// Initialize a new ray with the spcecified values
        /// </summary>
        /// <param name="position">Starting point</param>
        /// <param name="direction">Direction</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
            UnitDirection = Vector3.Normalize(direction);
        }

        /// <summary>
        /// Starting point of ray
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Direction of ray
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        /// Unit vector direction of ray
        /// </summary>
        public Vector3 UnitDirection { get; }

        public static Ray Transform(Ray ray, Matrix4x4 matrix)
        {
            var p = Vector3.Transform(ray.Position, matrix);
            var d = Vector3.TransformNormal(ray.Direction, matrix);
            return new Ray(p, d);
        }

        public bool Equals(Ray other) => Position.Equals(other.Position) && Direction.Equals(other.Direction) && UnitDirection.Equals(other.UnitDirection);

        public override bool Equals(object obj) => obj is Ray other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Position, Direction, UnitDirection);

        public static bool operator ==(Ray left, Ray right) => left.Equals(right);

        public static bool operator !=(Ray left, Ray right) => !left.Equals(right);
    }
}
