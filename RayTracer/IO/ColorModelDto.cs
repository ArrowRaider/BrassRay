using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer.IO
{
    internal class ColorModelDto
    {
        public decimal Gamma { get; set; } = 2.2M;
        public decimal InFactor { get; set; } = 1.0M;
        public decimal OutFactor { get; set; } = 1.0M;
        public decimal EnvironmentBackgroundFactor { get; set; } = 1.0M;
    }
}
