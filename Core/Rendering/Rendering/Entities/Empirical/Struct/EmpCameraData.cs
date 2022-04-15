using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Empirical
{
    [StructLayout(LayoutKind.Explicit, Size = 19 * sizeof(float))]
    public struct EmpCameraData : ICameraData
    {
        [FieldOffset( 0 * sizeof(float))] public Vector2i resolution;
        [FieldOffset( 2 * sizeof(float))] public float fov;
        [FieldOffset( 3 * sizeof(float))] public Matrix3 rotationMatrix;

        [FieldOffset(12 * sizeof(float))] public Vector2 focalLength;
        [FieldOffset(14 * sizeof(float))] public (float r0, float r1, float r2) radialDistortionCoefficient;
        [FieldOffset(17 * sizeof(float))] public Vector2 tangentialDistortionCoefficient;

        public Vector2i Resolution 
        {
            get => resolution; 
            set => resolution = value; 
        }

        public float FOV 
        {
            get => fov; 
            set => fov = value; 
        }

        public Matrix3 RotationMatrix 
        {
            get => rotationMatrix; 
            set => rotationMatrix = value; 
        }


        // x, y focal lengths
        public Vector2 FocalLength 
        {
            get => focalLength; 
            set => focalLength = value; 
        }

        // r^2, r^4, r^6 order radial distortion coefficients
        public (float r0, float r1, float r2) RadialDistortionCoefficient 
        {
            get => radialDistortionCoefficient; 
            set => radialDistortionCoefficient = value; 
        }

        // x, y tangential distortions
        public Vector2 TangentialDistortionCoefficient 
        {
            get => tangentialDistortionCoefficient; 
            set => tangentialDistortionCoefficient = value; 
        }

    }
}
