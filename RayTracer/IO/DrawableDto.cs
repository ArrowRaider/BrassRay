using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal abstract class DrawableDto
    {
        public Vector3 Position { get; set; }
        public MaterialHolder Material { get; set; }
        public List<TransformHolder> Transform { get; set; }
        public Projection TextureProjection { get; set; } = Projection.None;
    }

    internal class InfinitePlaneDto : DrawableDto
    {
        public Vector3 Normal { get; set; }
    }

    internal class BoxDto : DrawableDto
    {
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Depth { get; set; }
    }

    internal class SphereDto : DrawableDto
    {
        public decimal Radius { get; set; }
    }

    internal class CylinderDto : DrawableDto
    {
        public decimal Radius1 { get; set; }
        public decimal Radius2 { get; set; }
        public decimal Height { get; set; }
    }

    internal class DrawableHolder
    {
        public InfinitePlaneDto InfinitePlane { get; set; }
        public BoxDto Box { get; set; }
        public SphereDto Sphere { get; set; }
        public CylinderDto Cylinder { get; set; }
    }
}
