using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BrassRay.RayTracer.IO
{
    internal class TransformDto { }

    internal class ScaleTransformDto : TransformDto
    {
        public Vector3 Scale { get; set; }
        public Vector3? Center { get; set; }
    }

    // Represents a Euler XYZ rotation
    internal class RotateTransformDto : TransformDto
    {
        // X, Y, Z rotation angles in degrees
        public Vector3 Rotation { get; set; }
        public Vector3? Center { get; set; }
    }

    internal class TranslateTransformDto : TransformDto
    {
        public Vector3 Offset { get; set; }
    }

    // Represents a quaternion rotation
    internal class QuaternionTransformDto : TransformDto
    {
        // The angle of rotation in degrees
        public float Angle { get; set; }

        // The axis of rotation
        public Vector3 Axis { get; set; }

        public Vector3? Center { get; set; }
    }

    internal class MatrixTransformDto : TransformDto
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M14 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M24 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }
        public float M34 { get; set; }
        public float M41 { get; set; }
        public float M42 { get; set; }
        public float M43 { get; set; }
        public float M44 { get; set; }
    }

    internal class TransformHolder
    {
        public string Name { get; set; }
        public ScaleTransformDto ScaleTransform { get; set; }
        public RotateTransformDto RotateTransform { get; set; }
        public TranslateTransformDto TranslateTransform { get; set; }
        public QuaternionTransformDto QuaternionTransform { get; set; }
        public MatrixTransformDto MatrixTransform { get; set; }
        public List<TransformHolder> Children { get; set; }
    }
}
