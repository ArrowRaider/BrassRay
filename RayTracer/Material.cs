using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public abstract class Material
    {
        public string Name { get; set; }

        // not sure if this should be made virtual
        protected abstract Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth);

        public Vector3 Shade(Ray ray, Scene scene, Intersection p, int depth = Utils.DefaultDepth) =>
            depth <= 0 ? Vector3.Zero : ShadeCore(ray, scene, p, depth);
    }

    /// <summary>
    /// Solid color material
    /// </summary>
    public class EmissiveMaterial : Material
    {
        public Vector3 Color { get; set; }
        protected override Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth) => Color;
    }

    /// <summary>
    /// This is a debugging material
    /// </summary>
    public class FastDiffuseMaterial : Material
    {
        public Vector3 Color { get; set; }
        protected override Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth)
        {
            var d1 = new Vector3(0.0f, 0.0f, 1.0f);
            var m1 = MathF.Max(0.0f, Vector3.Dot(p.Normal.Direction, d1));
            var d2 = new Vector3(2.0f, 1.0f, -1.0f);
            var m2 = MathF.Max(0.0f, Vector3.Dot(p.Normal.Direction, d2));
            return (m1 * 0.6f + m2 * 0.4f) * Color;
        }
    }

    /// <summary>
    /// Stochastic diffuse
    /// </summary>
    public class LambertianMaterial : Material
    {
        public Vector3 Color { get; set; }

        protected override Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth)
        {
            var from = p.Normal.PerturbRay();
            var d = p.Normal.Direction + Utils.SphereRandom(RandomProvider.Random) * 2.0f;
            return scene.Shade(new Ray(from.Position, d), depth - 1) * Color;
        }
    }

    /// <summary>
    /// Reflect, with optional stochastic blur
    /// </summary>
    public class ReflectMaterial : Material
    {
        public Vector3 Color { get; set; }
        public float Scatter { get; set; }

        protected override Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth)
        {
            var from = p.Normal.PerturbRay();
            var d = -2.0f * Vector3.Dot(ray.Direction, p.Normal.Direction) * p.Normal.Direction + ray.Direction;
            if (Scatter > 0.0f)
                d += Utils.SphereRandom(RandomProvider.Random) * Scatter;
            return scene.Shade(new Ray(from.Position, d), depth - 1) * Color;
        }
    }

    /// <summary>
    /// Refract, with optional stochastic blur
    /// </summary>
    public class RefractMaterial : Material
    {
        public Vector3 Color { get; set; }
        public float Ior { get; set; } = 1.5f;
        public float Scatter { get; set; }

        protected override Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth)
        {
            if (depth <= 0)
                return Vector3.Zero;

            var from = p.Normal.PerturbRayNegative();
            var d = Scatter > 0.0 ? Vector3.Normalize(ray.Direction + Utils.SphereRandom(RandomProvider.Random) * Scatter) : ray.Direction;
            var rr = ray.Inside ? Ior : 1.0f / Ior;

            var c = Vector3.Dot(p.Normal.Direction, d);
            var s = 1 - rr * rr * (1 - c * c);

            // creating the secondary ray below
            Ray ray2;
            if (s < 0.0f)
            {
                // total internal refraction
                var d2 = -2.0f * Vector3.Dot(d, p.Normal.Direction) * p.Normal.Direction + d;
                ray2 = new Ray(from.Position, d2, ray.Inside);
            }
            else
            {
                // refract ray
                var d2 = (rr * -c - MathF.Sqrt(s)) * p.Normal.Direction + rr * d;
                ray2 = new Ray(from.Position, d2, !ray.Inside);
            }

            // if the ray is outside, then continue scene shading from secondary ray
            if (!ray2.Inside)
                return scene.Shade(ray2, depth - 1);

            // inside the object and need to find where the other end of it is
            var p2 = p.Drawable.Intersect(ray2);
            // need copy of ray without inside flag
            var ray3 = new Ray(ray2.Position, ray2.Direction);
            // need to check if there is another object between here and the other end of the current drawable
            var p3 = scene.ClosestIntersection(ray3);

            // condition:  the secondary ray hit nothing
            if (!p2.HasValue && !p3.HasValue)
                return scene.Background.Shade(ray2) * Color;

            // condition:  the secondary ray hit an object that is NOT the current drawable
            if (!p2.HasValue || p3.HasValue && p3.Value.T < p2.Value.T)
                return scene.Shade(ray3, depth - 1) * Color;

            // condition:  the secondary ray reached the other end of the current drawable, continue from there
            return p2.Value.Drawable.Material.Shade(ray2, scene, p2.Value, depth) * Color;
        }
    }

    /// <summary>
    /// Stochastic approximation of the fresnel effect.
    /// </summary>
    public class SchlickMaterial : Material
    {
        public SchlickMaterial()
        {
            Ior = 1.5f;
        }

        /// <summary>
        /// The material that is shown at high angles
        /// </summary>
        public Material High { get; set; }

        /// <summary>
        /// The material that is shown at shallow angles, nearly parallel to the surface.
        /// </summary>
        public Material Low { get; set; }

        public float Ior
        {
            get => _ior;
            set
            {
                _ior = value;
                _r0 = MathF.Pow((_ior - 1) / (_ior + 1), 2);
            }
        }

        private float _r0;
        private float _ior;

        protected override Vector3 ShadeCore(Ray ray, Scene scene, Intersection p, int depth)
        {
            var c = Vector3.Dot(ray.Direction, -p.Normal.Direction);
            var prob = _r0 + MathF.Pow(1 - c, 5) * (1 - _r0);
            var m = RandomProvider.Random.NextDouble() < prob ? Low : High;
            return m.Shade(ray, scene, p, depth);
        }
    }
}
