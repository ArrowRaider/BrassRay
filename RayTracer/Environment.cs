using System.Numerics;

namespace BrassRay.RayTracer
{
    /// <summary>
    /// Rays that intersect with nothing in the scene hit the environment instead
    /// </summary>
    public abstract class Environment
    {
        public abstract Vector3 Shade(in Ray ray);
    }

    /// <summary>
    /// Environment that is a single color
    /// </summary>
    public class SolidEnvironment : Environment
    {
        public SolidEnvironment() { }
        public SolidEnvironment(Vector3 color) => Color = color;

        public Vector3 Color { get; set; }

        public override Vector3 Shade(in Ray ray) => Color;
    }
}
