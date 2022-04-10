
using System.Runtime.InteropServices;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    [StructLayout(LayoutKind.Explicit, Size = 6 * sizeof(float))]
    public struct ScreenPoint
    {
        [FieldOffset(0 * sizeof(float))]
        public Vector2 screenPosition;

        [FieldOffset(2 * sizeof(float))]
        public Vector2i pixelPosition;
        
        [FieldOffset(4 * sizeof(float))]
        public bool isVisible;

        [FieldOffset(5 * sizeof(float))]
        public int pointID;
    }
}
