using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal abstract class CameraDto
    {
        public int BlockWidth { get; set; }
        public int BlockHeight { get; set; }
        public int PixelHeight { get; set; }
        public decimal Ratio { get; set; }
    }

    internal class TargetCameraDto : CameraDto
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public decimal FieldOfView { get; set; }
        public decimal Blur { get; set; }
    }

    internal class OrthographicCameraDto : CameraDto
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 Up { get; set; }
        public float ViewHeight { get; set; }
    }

    internal class SphericalCameraDto : CameraDto
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }

    internal class CameraHolder
    {
        public TargetCameraDto TargetCamera { get; set; }
        public OrthographicCameraDto OrthographicCamera { get; set; }
        public SphericalCameraDto SphericalCamera { get; set; }
    }
}
