using System;
using System.Collections.Generic;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal class SceneDto
    {
        public ColorModel ColorModel { get; set; } = new();
        public SamplerHolder Environment { get; set; }
        public List<DrawableHolder> Drawables { get; set; }
        public CameraHolder Camera { get; set; }
    }
}
