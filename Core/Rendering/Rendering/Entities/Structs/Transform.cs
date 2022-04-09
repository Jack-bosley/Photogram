using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    public struct Transform
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Transform()
        {
            position = new Vector3(0, 0, 0);
            rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1, 1, 1);
        }


        public Vector3 GetDirectionVector()
        {
            return GetRotationMatrix() * new Vector3(0, 0, 1);
        }

        public Matrix3 GetRotationMatrix()
        {
            float cx = MathF.Cos(rotation.X), sx = MathF.Sin(rotation.X);
            float cy = MathF.Cos(rotation.Y), sy = MathF.Sin(rotation.Y);
            float cz = MathF.Cos(rotation.Z), sz = MathF.Sin(rotation.Z);

            float m00 = cy * cz;
            float m01 = cy * sz;
            float m02 = -sy;

            float m10 = (sx * sy * cz) - (cx * sz);
            float m11 = (sx * sy * sz) + (cx * cz);
            float m12 = sx * cy;

            float m20 = (cx * sy * cz) + (sx * sz);
            float m21 = (sx * sy * sz) - (sx * cz);
            float m22 = cx * cy;

            return new Matrix3(m00, m10, m20, m01, m11, m21, m02, m12, m22);
        }
    }
}
