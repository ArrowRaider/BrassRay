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
        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = Vector3.Normalize(direction);
        }

        /// <summary>
        /// Starting point of ray
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Unit vector direction of ray
        /// </summary>
        public Vector3 Direction { get; }
    }
}
