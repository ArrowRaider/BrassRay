using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer
{
    public readonly struct Rgb
    {
        public Rgb(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public float R { get; }
        public float G { get; }
        public float B { get; }
    }

    public readonly struct ClampedRgb
    {
        public ClampedRgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public static explicit operator Rgb(in ClampedRgb x) => new Rgb(x.R, x.G, x.B);

        public static explicit operator ClampedRgb(Rgb x) =>
            new ClampedRgb((byte)MathF.Round(MathF.Min(1.0f, MathF.Max(0.0f, x.R)) * 255),
                (byte)MathF.Round(MathF.Min(1.0f, MathF.Max(0.0f, x.G)) * 255),
                (byte)MathF.Round(MathF.Min(1.0f, MathF.Max(0.0f, x.B)) * 255));

        public static explicit operator uint(in ClampedRgb x) => (uint)x.R << 16 | (uint)x.G << 8 | x.B;
    }
}
