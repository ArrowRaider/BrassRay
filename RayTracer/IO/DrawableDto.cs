using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal abstract class DrawableDto
    {
        public string Material { get; set; }
    }

    internal class InfinitePlaneDto : DrawableDto
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
    }

    internal class BoxDto : DrawableDto
    {
        public Vector3 Position { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Depth { get; set; }
    }

    internal class SphereDto : DrawableDto
    {
        public Vector3 Position { get; set; }
        public decimal Radius { get; set; }
    }

    internal class DrawableHolder
    {
        public InfinitePlaneDto InfinitePlane { get; set; }
        public BoxDto Box { get; set; }
        public SphereDto Sphere { get; set; }
    }
}
