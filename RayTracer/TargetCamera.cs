using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public class TargetCamera : Camera
    {
        public TargetCamera()
        {
        }

        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public float FieldOfView { get; set; }
        public float Blur { get; set; }

        protected override CoordinateSystem GetCoordinateSystem()
        {
            var diff = Position - Target;
            var n = diff / diff.Length();
            var u = Vector3.Normalize(Vector3.Cross(Up, n));
            var v = Vector3.Cross(n, u);

            var theta = FieldOfView * MathF.PI / 180.0f;
            var h = 2.0f * diff.Length() * MathF.Tan(theta / 2.0f);
            var w = Ratio * h;
            var interval = h / (PixelHeight - 1.0f);

            var origin = Target + u * w / 2 + v * h / 2;

            return new CoordinateSystem(origin, u, v, new Vector2(interval));
        }

        protected override Ray GetCameraRay(Vector3 target, in CoordinateSystem cs)
        {
            var d0 = Position;
            if (Blur > 0.0)
            {
                var b = Utils.DiscRandom(RandomProvider.Random) * Blur;
                d0 += b.X * cs.U + b.Y * cs.V;
            }

            return new Ray(d0, Vector3.Normalize(target - d0));
        }
    }
}
