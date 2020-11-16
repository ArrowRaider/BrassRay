using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public class SphericalCamera : Camera
    {
        public Vector3 Position { get; set; }

        protected override CoordinateSystem GetCoordinateSystem()
        {
            return new CoordinateSystem(Position + new Vector3(MathF.PI, MathF.PI / 2.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f),
                new Vector2(2.0f * MathF.PI / PixelWidth, MathF.PI / PixelHeight));
        }

        protected override Ray GetCameraRay(Vector3 target, in CoordinateSystem cs)
        {
            var diff = target - Position;
            var rY = MathF.Cos(diff.Y);
            var d = new Vector3(rY * MathF.Cos(diff.X), MathF.Sin(diff.Y), rY * MathF.Sin(diff.X));
            return new Ray(Position, d);
        }
    }
}
