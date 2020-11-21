using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer.IO
{
    internal abstract class SamplerDto
    {

    }

    internal class SolidSamplerDto : SamplerDto
    {
        public dynamic Color { get; set; }
    }

    internal class CheckerSamplerDto : SamplerDto
    {
        public SamplerHolder Color1 { get; set; }
        public SamplerHolder Color2 { get; set; }
    }

    internal class SkySamplerDto : SamplerDto
    {
        public SamplerHolder HighColor { get; set; }
        public SamplerHolder LowColor { get; set; }
        public SamplerHolder SunColor { get; set; }
        public string SunDirection { get; set; }
    }

    internal class RainbowSamplerDto : SamplerDto
    {
        public SamplerHolder XColor { get; set; }
        public SamplerHolder YColor { get; set; }
        public SamplerHolder ZColor { get; set; }
        public Vector3 Scale { get; set; }
    }

    internal class SamplerHolder
    {
        public SolidSamplerDto SolidSampler { get; set; }
        public CheckerSamplerDto CheckerSampler { get; set; }
        public SkySamplerDto SkySampler { get; set; }
        public RainbowSamplerDto RainbowSampler { get; set; }
    }
}
