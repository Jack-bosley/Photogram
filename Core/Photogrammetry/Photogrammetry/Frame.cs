using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

using Core.Rendering.Entities;

namespace Core.Photogrammetry
{
    public class Frame
    {
        public PointImage FramePointImage { get; set; }
        public Transform CameraOrientation { get; set; }

        public Frame(Vector2i frameResolution, ScreenPoint[] screenPoints)
        {
            FramePointImage = new PointImage(frameResolution, screenPoints);
        }
    }
}
