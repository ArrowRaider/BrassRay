using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public ref struct ShadeState
    {
        public ShadeState(int depth, Sobol.Sequence sobolSequence)
        {
            Depth = depth;
            SobolSequence = sobolSequence;
        }

        public int Depth { get; set; }
        public Sobol.Sequence SobolSequence { get; }
    }
}
