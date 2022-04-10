using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

using Core.Rendering.Entities;

namespace Core.Photogrammetry
{
    public class PointImage
    {
        public Vector2i Resulution { get; set; }
        public ScreenPoint[] ImagePoints { get; set; }

        public PointImage(Vector2i resulution, ScreenPoint[] imagePoints)
        {
            Resulution = resulution;
            ImagePoints = imagePoints;
        }

        public IEnumerable<int> GetPointIDs()
        {
            foreach (ScreenPoint point in ImagePoints)
                yield return point.pointID;
        }
    }
}
