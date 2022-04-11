using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Photogrammetry
{
    [StructLayout(LayoutKind.Explicit, Size = 3 * sizeof(float))]
    internal struct ScreenPointError
    {
        [FieldOffset(0 * sizeof(float))]
        public Vector2 errorVector;

        [FieldOffset(2 * sizeof(float))]
        public float errorRadius;
    }
}
