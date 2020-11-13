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

        public static explicit operator Vector3(in Rgb x) => new Vector3(MathF.Pow(x.R, Utils.Gamma),
            MathF.Pow(x.G, Utils.Gamma), MathF.Pow(x.B, Utils.Gamma));

        public static explicit operator Rgb(Vector3 x) => new Rgb(MathF.Pow(x.X, 1.0f / Utils.Gamma),
            MathF.Pow(x.Y, 1.0f / Utils.Gamma), MathF.Pow(x.Z, 1.0f / Utils.Gamma));
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

        public static explicit operator Vector3(in ClampedRgb x) => new Vector3(MathF.Pow(x.R / 255.0f, Utils.Gamma),
            MathF.Pow(x.G / 255.0f, Utils.Gamma), MathF.Pow(x.B / 255.0f, Utils.Gamma));

        public static explicit operator ClampedRgb(Vector3 x)
        {
            var y = Vector3.Clamp(
                new Vector3(MathF.Pow(x.X, 1.0f / Utils.Gamma), MathF.Pow(x.Y, 1.0f / Utils.Gamma),
                    MathF.Pow(x.Z, 1.0f / Utils.Gamma)), Vector3.Zero, Vector3.One);
            return new ClampedRgb((byte)MathF.Round(y.X * 255), (byte)MathF.Round(y.Y * 255),
                (byte)MathF.Round(y.Z * 255));
        }

        public static explicit operator uint(in ClampedRgb x) => (uint)x.R << 16 | (uint)x.G << 8 | x.B;
    }
}
