using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal abstract class EnvironmentDto { }

    internal class SolidEnvironmentDto : EnvironmentDto
    {
        public Rgb Color { get; set; }
    }

    internal class SkyEnvironmentDto : EnvironmentDto
    {
        public Rgb HighColor { get; set; }
        public Rgb LowColor { get; set; }
        public Rgb SunColor { get; set; }
        public decimal SunFalloff { get; set; }
        public Vector3 SunDirection { get; set; }
    }

    internal class RainbowEnvironmentDto : EnvironmentDto
    {
    }

    internal class EnvironmentHolder
    {
        public SolidEnvironmentDto SolidEnvironment { get; set; }
        public SkyEnvironmentDto SkyEnvironment { get; set; }
        public RainbowEnvironmentDto RainbowEnvironment { get; set; }
    }
}
