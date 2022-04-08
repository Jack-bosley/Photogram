using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Rays
{
    public struct Ray
    {
        public const int SIZE = 20 * sizeof(float);

        public Vector4 origin;
        public Vector4 direction;
        public Vector4 colour;
        public Vector2i pixel;

        public float nearPointCutoff;
        public float farPointCutoff;
        public bool isReflection;

        private Vector3 padding;
    }
}
