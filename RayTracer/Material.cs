using System;
using System.Diagnostics;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public abstract class Material
    {
        public string Name { get; set; }

        // not sure if this should be made virtual
        protected abstract Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth);

        public Vector3 Shade(in Ray ray, Scene scene, in Intersection p, int depth = Utils.DefaultDepth) =>
            depth <= 0 ? Vector3.Zero : ShadeCore(ray, scene, p, depth);
    }

    /// <summary>
    /// Solid color material
    /// </summary>
    public class EmissiveMaterial : Material
    {
        public Sampler Color { get; set; }
        protected override Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth) => Color.Sample(p.Position);
    }

    /// <summary>
    /// This is a debugging material
    /// </summary>
    public class FastDiffuseMaterial : Material
    {
        public Sampler Color { get; set; }
        protected override Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth)
        {
            var d1 = new Vector3(0.0f, 0.0f, 1.0f);
            var m1 = MathF.Max(0.0f, Vector3.Dot(p.Normal, d1));
            var d2 = new Vector3(2.0f, 1.0f, -1.0f);
            var m2 = MathF.Max(0.0f, Vector3.Dot(p.Normal, d2));
            return (m1 * 0.6f + m2 * 0.4f) * Color.Sample(p.Position);
        }
    }

    /// <summary>
    /// Stochastic diffuse
    /// </summary>
    public class LambertianMaterial : Material
    {
        public Sampler Color { get; set; }

        protected override Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth)
        {
            var from = p.Position + p.Normal * Utils.Epsilon;
            var d = p.Normal + Utils.SphereRandom(RandomProvider.Random) * 2.0f;
            return scene.Shade(new Ray(from, d), depth - 1) * Color.Sample(p.Position);
        }
    }

    /// <summary>
    /// Reflect, with optional stochastic blur
    /// </summary>
    public class ReflectMaterial : Material
    {
        public Sampler Color { get; set; }
        public float Scatter { get; set; }

        protected override Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth)
        {
            var from = p.Position + p.Normal * Utils.Epsilon;
            var d = -2.0f * Vector3.Dot(ray.UnitDirection, p.Normal) * p.Normal + ray.UnitDirection;
            if (Scatter > 0.0f)
                d += Utils.SphereRandom(RandomProvider.Random) * Scatter;
            return scene.Shade(new Ray(from, d), depth - 1) * Color.Sample(p.Position);
        }
    }

    /// <summary>
    /// Refract, with optional stochastic blur
    /// </summary>
    public class RefractMaterial : Material
    {
        public Sampler Color { get; set; }
        public float Ior { get; set; } = 1.5f;
        public float Scatter { get; set; }

        protected override Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth)
        {
            var from = p.Position - p.Normal * Utils.Epsilon;
            var d = Scatter > 0.0f ? Vector3.Normalize(ray.UnitDirection + Utils.SphereRandom(RandomProvider.Random) * Scatter) : ray.UnitDirection;
            var refRatio = p.Inside ? Ior : 1.0f / Ior;
            var c = Vector3.Dot(p.Normal, d);
            var s = 1 - refRatio * refRatio * (1 - c * c);

            // creating the secondary ray below
            Ray ray2;
            if (s < Utils.Epsilon)
            {
                // total internal refraction
                var d2 = -2.0f * Vector3.Dot(d, p.Normal) * p.Normal + d;
                ray2 = new Ray(from, d2);
            }
            else
            {
                // refract ray
                var d2 = (refRatio * -c - MathF.Sqrt(s)) * p.Normal + refRatio * d;
                ray2 = new Ray(from, d2);
            }

            var color = Color.Sample(p.Position);
            color = new Vector3(MathF.Sqrt(color.X), MathF.Sqrt(color.Y), MathF.Sqrt(color.Z));
            // get shade of secondary ray
            return scene.Shade(ray2, depth - 1) * color;
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

        protected override Vector3 ShadeCore(in Ray ray, Scene scene, in Intersection p, int depth)
        {
            var c = Vector3.Dot(ray.UnitDirection, -p.Normal);
            var prob = _r0 + MathF.Pow(1 - c, 5) * (1 - _r0);
            var m = RandomProvider.Random.NextDouble() < prob ? Low : High;
            return m.Shade(ray, scene, p, depth);
        }
    }
}
