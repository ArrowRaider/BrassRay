using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public class RainbowEnvironment : Environment
    {
        public override Vector3 Shade(in Ray ray)
        {
            throw new NotImplementedException();
            //return (Vector3)new Rgb((-ray.UnitDirection.X + 1) / 2.0f, (ray.UnitDirection.Z + 1) / 2.0f,
            //    (ray.UnitDirection.Y + 1) / 2.0f);
        }
    }
}
