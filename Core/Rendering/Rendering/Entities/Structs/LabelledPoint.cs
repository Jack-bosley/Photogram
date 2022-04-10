
using System.Runtime.InteropServices;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * sizeof(float))]
    public struct LabelledPoint
    {
        [FieldOffset(0 * sizeof(float))]
        public Vector3 position;

        [FieldOffset(3 * sizeof(float))]
        public int pointID;
    }
}
