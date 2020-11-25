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
        private Matrix4x4 _matrix;
        private Vector3 _rotation;
        public Vector3 Position { get; set; }

        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _matrix = Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            }
        }

        protected override CoordinateSystem GetCoordinateSystem()
        {
            return new (new Vector3(MathF.PI, MathF.PI / 2.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f),
                new Vector2(2.0f * MathF.PI / PixelWidth, MathF.PI / PixelHeight));
        }

        protected override Ray GetCameraRay(Vector3 target, in CoordinateSystem cs, in Sobol.Sequence sobolSequence)
        {
            var rY = MathF.Cos(target.Y);
            var d = new Vector3(rY * MathF.Cos(target.X), MathF.Sin(target.Y), rY * MathF.Sin(target.X));
            d = Vector3.TransformNormal(d, _matrix);
            return new Ray(Position, d);
        }
    }
}
