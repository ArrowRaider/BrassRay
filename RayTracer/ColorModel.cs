using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public class ColorModel
    {
        public float Gamma { get; set; } = 2.2f;
        public float InFactor { get; set; } = 1.0f;
        public float OutFactor { get; set; } = 1.0f;
        public float EnvironmentBackgroundFactor { get; set; } = 1.0f;

        public Vector3 RgbToVector(in Rgb rgb) =>
            new Vector3(MathF.Pow(rgb.R, Gamma),
                MathF.Pow(rgb.G, Gamma), MathF.Pow(rgb.B, Gamma)) * InFactor;

        public Rgb VectorToRgb(Vector3 vector)
        {
            var v = vector / InFactor / InFactor * OutFactor;
            return new Rgb(MathF.Pow(v.X, 1.0f / Gamma),
                MathF.Pow(v.Y, 1.0f / Gamma), MathF.Pow(v.Z, 1.0f / Gamma));
        }
    }
}
