using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public class OrthographicCamera : Camera
    {
        private Vector3 _direction;
        public Vector3 Position { get; set; }
        public Vector3 Direction
        {
            get => _direction;
            set => _direction = Vector3.Normalize(value);
        }
        public Vector3 Up { get; set; }
        public float ViewHeight { get; set; }

        protected override CoordinateSystem GetCoordinateSystem()
        {
            var n = -Direction;
            var u = Vector3.Normalize(Vector3.Cross(Up, n));
            var v = Vector3.Cross(n, u);

            var h = ViewHeight;
            var w = Ratio * ViewHeight;
            var interval = h / (PixelHeight - 1.0f);

            var origin = Position + u * w / 2 + v * h / 2;

            return new CoordinateSystem(origin, u, v, new Vector2(interval));
        }

        protected override Ray GetCameraRay(Vector3 target, in CoordinateSystem cs) => new Ray(target, Direction);
    }
}
