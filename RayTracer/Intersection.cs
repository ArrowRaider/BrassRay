using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Describes the intersection of a ray and a drawable
    /// </summary>
    public readonly struct Intersection
    {
        public Intersection(float t, Vector3 position, Vector3 normal, bool inside, Drawable drawable)
        {
            T = t;
            Position = position;
            Normal = Vector3.Normalize(normal);
            Inside = inside;
            Drawable = drawable;
        }

        /// <summary>
        /// The distance from the ray position to the point of intersection
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

        /// <summary>
        /// Whether ray originated inside the drawable or not
        /// </summary>
        public bool Inside { get; }

        /// <summary>
        /// The drawable that intersects with the ray
        /// </summary>
        public Drawable Drawable { get; }
    }
}
