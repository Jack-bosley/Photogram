using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Photogrammetry
{
    public struct ImagePoint
    {
        public Vector2 screenPosition;
        public Vector2i pixelPosition;
        public int pointID;
    }
}
