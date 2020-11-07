namespace BrassRay.RayTracer
{
    /// <summary>
    /// Describes the intersection of a ray and a drawable
    /// </summary>
    public readonly struct Intersection
    {
        public Intersection(float t, Ray normal, Drawable drawable)
        {
            T = t;
            Normal = normal;
            Drawable = drawable;
        }

        /// <summary>
        /// The distance from the ray position to the point of intersection
        /// </summary>
        public float T { get; }

        /// <summary>
        /// Contains the point where the intersection occurs and the normal direction
        /// </summary>
        public Ray Normal { get; }

        /// <summary>
        /// The drawable that intersects with the ray
        /// </summary>
        public Drawable Drawable { get; }
    }
}
