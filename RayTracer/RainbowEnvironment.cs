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
        public Vector3 XColor { get; set; }
        public Vector3 YColor { get; set; }
        public Vector3 ZColor { get; set; }
        public Vector3 Scale { get; set; }

        public override Vector3 Shade(in Ray ray)
        {
            var d = Scale * ray.UnitDirection;
            return (d.X + 1) / 2.0f * XColor +
                   (d.Y + 1) / 2.0f * YColor +
                   (d.Z + 1) / 2.0f * ZColor;
        }
    }
}
