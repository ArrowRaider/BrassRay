using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal abstract class MaterialDto { }

    internal class EmissiveMaterialDto : MaterialDto
    {
        public SamplerHolder Color { get; set; }
    }

    internal class FastDiffuseMaterialDto : MaterialDto
    {
        public SamplerHolder Color { get; set; }
    }

    internal class LambertianMaterialDto : MaterialDto
    {
        public SamplerHolder Color { get; set; }
    }

    internal class ReflectMaterialDto : MaterialDto
    {
        public SamplerHolder Color { get; set; }
        public decimal Scatter { get; set; }
    }

    internal class RefractMaterialDto : MaterialDto
    {
        public SamplerHolder Color { get; set; }
        public decimal Ior { get; set; }
        public decimal Scatter { get; set; }
    }

    internal class SchlickMaterialDto : MaterialDto
    {
        public MaterialHolder High { get; set; }
        public MaterialHolder Low { get; set; }
        public decimal Ior { get; set; }
    }

    internal class MaterialHolder
    {
        public string Name { get; set; }
        public EmissiveMaterialDto EmissiveMaterial { get; set; }
        public FastDiffuseMaterialDto FastDiffuseMaterial { get; set; }
        public LambertianMaterialDto LambertianMaterial { get; set; }
        public ReflectMaterialDto ReflectMaterial { get; set; }
        public RefractMaterialDto RefractMaterial { get; set; }
        public SchlickMaterialDto SchlickMaterial { get; set; }
    }
}
