using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public class Scene
    {
        public List<Drawable> Drawables { get; } = new List<Drawable>();
        public Background Background { get; set; }
        public Camera Camera { get; set; }

        public Intersection? ClosestIntersection(Ray ray)
        {
            // TODO:  Build a k-d tree to efficiently search the scene graph

            Intersection? result = null;
            foreach (var p in Drawables.Select(drawable => drawable.Intersect(ray))
                .Where(m => m.HasValue && !(m.Value.T < 0.0))
                .Where(m => !result.HasValue || m.Value.T < result.Value.T))
                result = p;

            return result;
        }

        public Vector3 Shade(Ray ray, int depth = Utils.DefaultDepth)
        {
            if (depth <= 0)
                return Vector3.Zero;

            var m = ClosestIntersection(ray);
            return m?.Drawable.Material.Shade(ray, this, m.Value, depth) ?? Background.Shade(ray);
        }
    }
}
