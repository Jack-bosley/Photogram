using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Photogrammetry
{
    [StructLayout(LayoutKind.Explicit, Size = 18 * sizeof(float))]
    internal struct Jacobian
    {
        [FieldOffset( 0 * sizeof(float))] Matrix2x3 jE; // Camera position 
        [FieldOffset( 6 * sizeof(float))] Matrix2x3 jC; // Camera rotation
        [FieldOffset(12 * sizeof(float))] Matrix2x3 jX; // Point position
    }
}
