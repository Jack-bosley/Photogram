using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Rays
{
    [StructLayout(LayoutKind.Explicit, Size = 14 * sizeof(float))]
    public struct RayCameraData : ICameraData
    {
        [FieldOffset(0 * sizeof(float))] public Vector2i resolution;
        [FieldOffset(2 * sizeof(float))] public float fov;
        [FieldOffset(3 * sizeof(float))] public Matrix3 rotationMatrix;
        [FieldOffset(12 * sizeof(float))] public float nearPointCutoffDistance;
        [FieldOffset(13 * sizeof(float))] public float farPointCutoffDistance;



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


        public float NearPointCutoffDistance 
        { 
            get => nearPointCutoffDistance;
            set => nearPointCutoffDistance = value; 
        }

        public float FarPointCutoffDistance 
        { 
            get => farPointCutoffDistance;
            set => farPointCutoffDistance = value; 
        }

    }
}
