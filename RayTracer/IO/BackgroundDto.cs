using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal abstract class BackgroundDto { }

    internal class SolidBackgroundDto : BackgroundDto
    {
        public Rgb Color { get; set; }
    }

    internal class SkyBackgroundDto : BackgroundDto
    {
        public Rgb HighColor { get; set; }
        public Rgb LowColor { get; set; }
        public Rgb SunColor { get; set; }
        public decimal SunFalloff { get; set; }
        public Vector3 SunDirection { get; set; }
    }

    internal class BackgroundHolder
    {
        public SolidBackgroundDto SolidBackground { get; set; }
        public SkyBackgroundDto SkyBackground { get; set; }
    }
}
