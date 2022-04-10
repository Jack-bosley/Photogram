using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Photogrammetry
{
    public class PointImage
    {
        public Vector2i Resulution { get; set; }
        public ImagePoint[] ImagePoints { get; set; }

        public PointImage(Vector2i resulution, ImagePoint[] imagePoints)
        {
            Resulution = resulution;
            ImagePoints = imagePoints;
        }
    }
}
