using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    [StructLayout(LayoutKind.Explicit, Size = 9 * sizeof(float))]
    public struct Transform
    {
        [FieldOffset(0 * sizeof(float))]
        public Vector3 position;

        [FieldOffset(3 * sizeof(float))]
        public Vector3 rotation;

        [FieldOffset(6 * sizeof(float))]
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

        public (Matrix3 x, Matrix3 y, Matrix3 z) GetRotationEulerVectorDerivative()
        {
            Matrix3 x = GetRotationEulerVectorDerivative_x();
            Matrix3 y = GetRotationEulerVectorDerivative_y();
            Matrix3 z = GetRotationEulerVectorDerivative_z();

            return (x, y, z);
        }

        public Matrix3 GetRotationEulerVectorDerivative_x()
        {
            float cx = MathF.Cos(rotation.X), sx = MathF.Sin(rotation.X);
            float cy = MathF.Cos(rotation.Y), sy = MathF.Sin(rotation.Y);
            float cz = MathF.Cos(rotation.Z), sz = MathF.Sin(rotation.Z);

            // sx => cx, cx => -sx, 0 if no x dependence 
            // brackets show changed variables so ignore warnings
#pragma warning disable IDE0047
            float m00 = (0);
            float m01 = (0);
            float m02 = (0);

            float m10 = ((cx) * sy * cz) - ((-sx) * sz);
            float m11 = ((cx) * sy * sz) + ((-sx) * cz);
            float m12 = (cx) * cy;

            float m20 = ((-sx) * sy * cz) + ((cx) * sz);
            float m21 = ((cx) * sy * sz) - ((cx) * cz);
            float m22 = (-sx) * cy;
#pragma warning restore IDE0047

            return new Matrix3(m00, m10, m20, m01, m11, m21, m02, m12, m22);
        }
        public Matrix3 GetRotationEulerVectorDerivative_y()
        {
            float cx = MathF.Cos(rotation.X), sx = MathF.Sin(rotation.X);
            float cy = MathF.Cos(rotation.Y), sy = MathF.Sin(rotation.Y);
            float cz = MathF.Cos(rotation.Z), sz = MathF.Sin(rotation.Z);

            // sy => cy, cy => -sy, 0 if no y dependence 
            // brackets show changed variables so ignore warnings
#pragma warning disable IDE0047
            float m00 = (-sy) * cz;
            float m01 = (-sy) * sz;
            float m02 = -(cy);

            float m10 = (sx * (cy) * cz) - (0);
            float m11 = (sx * (cy) * sz) + (0);
            float m12 = sx * (-sy);

            float m20 = (cx * (cy) * cz) + (0);
            float m21 = (sx * (cy) * sz) - (0);
            float m22 = cx * (-sy);
#pragma warning restore IDE0047

            return new Matrix3(m00, m10, m20, m01, m11, m21, m02, m12, m22);
        }
        public Matrix3 GetRotationEulerVectorDerivative_z()
        {
            float cx = MathF.Cos(rotation.X), sx = MathF.Sin(rotation.X);
            float cy = MathF.Cos(rotation.Y), sy = MathF.Sin(rotation.Y);
            float cz = MathF.Cos(rotation.Z), sz = MathF.Sin(rotation.Z);

            // sz => cz, cz => -sz, 0 if no z dependence 
            // brackets show changed variables so ignore warnings
#pragma warning disable IDE0047
            float m00 = cy * (-sz);
            float m01 = cy * (cz);
            float m02 = (0);

            float m10 = (sx * sy * (-sz)) - (cx * (cz));
            float m11 = (sx * sy * (cz)) + (cx * (-sz));
            float m12 = (0);

            float m20 = (cx * sy * (-sz)) + (sx * (cz));
            float m21 = (sx * sy * (cz)) - (sx * (-sz));
            float m22 = (0);
#pragma warning restore IDE0047

            return new Matrix3(m00, m10, m20, m01, m11, m21, m02, m12, m22);
        }
    }
}
