using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    [StructLayout(LayoutKind.Explicit, Size = 12 * sizeof(float))]
    public struct ScreenPoint
    {
        [FieldOffset(0 * sizeof(float))]
        public Vector2 screenPosition;

        [FieldOffset(4 * sizeof(float))]
        public Vector2i pixelPosition;
        
        [FieldOffset(8 * sizeof(float))]
        public bool isVisible;
    }
}
