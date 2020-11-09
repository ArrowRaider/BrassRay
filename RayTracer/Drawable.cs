namespace BrassRay.RayTracer
{
    public abstract class Drawable
    {
        public Material Material { get; set; }

        // not sure if this should be made virtual
        /// <summary>
        /// Derived classes must provide the logic that calculates the intersection between a ray and this drawable
        /// </summary>
        /// <returns>Details about the point of intersection, if exists, null otherwise</returns>
        protected abstract Intersection? IntersectCore(Ray ray);

        /// <summary>
        /// Derived classes must provide the logic that calculates the intersection between a ray and this drawable
        /// </summary>
        /// <returns>Details about the point of intersection, if exists, null otherwise</returns>
        public Intersection? Intersect(Ray ray)
        {
            // TODO: object space transform
            return IntersectCore(ray);
        }
    }
}
