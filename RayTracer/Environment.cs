using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Rays that intersect with nothing in the scene hit the environment instead
    /// </summary>
    public class Environment
    {
        public Sampler Color { get; set; }

        public Vector3 Shade(in Ray ray) => Color.Sample(ray.UnitDirection);
    }
}
