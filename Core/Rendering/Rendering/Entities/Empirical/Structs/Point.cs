using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Empirical
{
    public struct Point
    {
        public const int SIZE = 12 * sizeof(float);

        public Vector4 worldPosition;
        public Vector2 screenPosition;
        public Vector2i pixelPosition;
        public bool isVisible;

        private Vector3 padding;
    }
}
