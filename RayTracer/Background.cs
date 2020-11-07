using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Rays that intersect with nothing in the scene hit a background instead
    /// </summary>
    public abstract class Background
    {
        public abstract Vector3 Shade(Ray ray);
    }

    /// <summary>
    /// Background that is a single color
    /// </summary>
    public class SolidBackground : Background
    {
        public SolidBackground() { }
        public SolidBackground(Vector3 color) => Color = color;

        public Vector3 Color { get; set; }

        public override Vector3 Shade(Ray ray) => Color;
    }
}
