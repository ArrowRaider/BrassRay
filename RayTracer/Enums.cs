using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public enum Projection
    {
        None,
        World,
        Object,
        Sphere,
        Cylinder,
        Box,
        PlaneXy,
        PlaneYz,
        PlaneZx
    }
}
