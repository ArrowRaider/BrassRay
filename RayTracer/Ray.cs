using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Represents an infinite ray described by a starting point (Position) and unit vector direction
    /// </summary>
    public readonly struct Ray
    {
        /// <summary>
        /// Initialize a new ray with the spcecified values
        /// </summary>
        /// <param name="position">Starting point</param>
        /// <param name="direction">Direction, will be converted to a unit vector</param>
        /// <param name="inside">Flip normals</param>
        public Ray(Vector3 position, Vector3 direction, bool inside = false)
        {
            Position = position;
            Direction = Vector3.Normalize(direction);
            Inside = inside;
        }

        /// <summary>
        /// Starting point of ray
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Unit vector direction of ray
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        /// Whether to flip normals or not
        /// </summary>
        public bool Inside { get; }

        /// <summary>
        /// Unary negates Direction and Inside
        /// </summary>
        /// <returns></returns>
        public static Ray operator -(Ray ray) => new Ray(ray.Position, -ray.Direction, !ray.Inside);

        public Ray PerturbRay() => new Ray(Position + Direction * Utils.Epsilon, Direction, Inside);
        public Ray PerturbRayNegative() => new Ray(Position - Direction * Utils.Epsilon, Direction, Inside);
    }
}
