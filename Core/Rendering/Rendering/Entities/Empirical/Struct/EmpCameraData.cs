using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Empirical
{
    public struct EmpCameraData : ICameraData
    {
        public Vector2i Resolution { get; set; }
        public float FOV { get; set; }

        // x, y focal lengths
        public Vector2 FocalLength { get; set; }

        // r^2, r^4, r^6 order radial distortion coefficients
        public (float r0, float r1, float r2) RadialDistortionCoefficient { get; set; }

        // x, y tangential distortions
        public Vector2 TangentialDistortionCoefficient { get; set; }
    }
}
