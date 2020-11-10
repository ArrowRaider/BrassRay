using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Two-color gradient environment with a sun
    /// </summary>
    public class SkyEnvironment : Environment
    {
        public SkyEnvironment() { }
        public SkyEnvironment(Vector3 highColor, Vector3 lowColor, Vector3 sunColor, Vector3 sunDirection, float sunFalloff)
        {
            HighColor = highColor;
            LowColor = lowColor;
            SunColor = sunColor;
            SunDirection = sunDirection;
            SunFalloff = sunFalloff;
        }

        private Vector3 _sunDirection;

        public Vector3 HighColor { get; set; }
        public Vector3 LowColor { get; set; }
        public Vector3 SunColor { get; set; }
        public Vector3 SunDirection
        {
            get => _sunDirection;
            set => _sunDirection = Vector3.Normalize(value);
        }

        public float SunFalloff { get; set; } = 120.0f;

        public override Vector3 Shade(Ray ray)
        {
            var x = Vector3.Dot(ray.UnitDirection, SunDirection);

            // TODO:  Don't hardcode the condition 0.9.  The reason I am doing this at all is because the calculuation is expensive.
            var sun = x > 0.9f ? SunColor / (1.0f + MathF.Pow(MathF.E, -SunFalloff * (x - 1.0f))) : Vector3.Zero;
            var t = MathF.Max(0.0f, MathF.Min(1.0f, 0.35f + ray.UnitDirection.Y * 2.0f));
            var b = (1.0f - t) * LowColor + t * HighColor;
            return b + sun;
        }
    }
}
