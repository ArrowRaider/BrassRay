using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public abstract class Sampler
    {
        private Matrix4x4 _transform = Matrix4x4.Identity;
        private Matrix4x4 _inverseTransform = Matrix4x4.Identity;
        public Matrix4x4 Transform
        {
            get => _transform;
            set
            {
                _transform = value;
                if (!Matrix4x4.Invert(_transform, out _inverseTransform))
                    throw new InvalidOperationException();
            }
        }

        public Vector3 Sample(Vector3 point)
        {
            var transformed = Vector3.Transform(point, _inverseTransform);
            return SampleCore(transformed);
        }

        protected abstract Vector3 SampleCore(Vector3 point);
    }

    public class SolidSampler : Sampler
    {
        public Vector3 Color { get; set; }

        protected override Vector3 SampleCore(Vector3 point) => Color;
    }

    public class CheckerSampler : Sampler
    {
        public Sampler Color1 { get; set; }
        public Sampler Color2 { get; set; }

        protected override Vector3 SampleCore(Vector3 point)
        {
            var u = ((int)MathF.Floor(point.X * 2 + Utils.Epsilon) & 1) == 0;
            var v = ((int)MathF.Floor(point.Y * 2 + Utils.Epsilon) & 1) == 0;
            var w = ((int)MathF.Floor(point.Z * 2 + Utils.Epsilon) & 1) == 0;
            return w == (u != v) ? Color1.Sample(point) : Color2.Sample(point);
        }
    }

    public class SkySampler : Sampler
    {
        private Vector3 _sunDirection;

        public Sampler HighColor { get; set; }
        public Sampler LowColor { get; set; }
        public Sampler SunColor { get; set; }
        public Vector3 SunDirection
        {
            get => _sunDirection;
            set => _sunDirection = Vector3.Normalize(value);
        }

        public float SunFalloff { get; set; } = 120.0f;

        protected override Vector3 SampleCore(Vector3 point)
        {
            var x = Vector3.Dot(point, SunDirection);

            // TODO:  Don't hardcode the condition 0.9.  The reason I am doing this at all is because the calculuation is expensive.
            var sun = x > 0.9f ? SunColor.Sample(point) / (1.0f + MathF.Pow(MathF.E, -SunFalloff * (x - 1.0f))) : Vector3.Zero;
            var t = MathF.Max(0.0f, MathF.Min(1.0f, 0.35f + point.Y * 2.0f));
            var b = (1.0f - t) * LowColor.Sample(point) + t * HighColor.Sample(point);
            return b + sun;
        }
    }

    public class RainbowSampler : Sampler
    {
        public Sampler XColor { get; set; }
        public Sampler YColor { get; set; }
        public Sampler ZColor { get; set; }
        public Vector3 Scale { get; set; }

        protected override Vector3 SampleCore(Vector3 point)
        {
            var d = Scale * point;
            return (d.X + 1) / 2.0f * XColor.Sample(point) +
                   (d.Y + 1) / 2.0f * YColor.Sample(point) +
                   (d.Z + 1) / 2.0f * ZColor.Sample(point);
        }
    }
}
