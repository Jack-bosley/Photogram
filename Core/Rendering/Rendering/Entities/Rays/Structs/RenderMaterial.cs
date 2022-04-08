using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Rays
{
    public struct RenderMaterial
    {
        public const int SIZE = 8 * sizeof(float);

        public Color4 color;
        public float reflectivity;

        private Vector3 padding;
    }
}
