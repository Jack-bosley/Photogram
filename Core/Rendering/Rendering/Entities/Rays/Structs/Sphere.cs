using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Rays
{
    public struct Sphere
    {
        public const int SIZE = 8 * sizeof(float);

        public Vector4 origin;
        public float radius;
        public int material;

        private Vector2 padding;
    }
}
