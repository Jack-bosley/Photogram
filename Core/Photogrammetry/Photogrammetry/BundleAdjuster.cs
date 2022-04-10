using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Rendering.Entities.Empirical;
using Core.Rendering.Entities;

namespace Core.Photogrammetry
{
    public class BundleAdjuster
    {
        public PointImage[] PointImages { get; set; }

        public CameraPose[] CameraPosePrediction { get; set; }
        public Point[] PointPrediction { get; set; }


        private EmpCamera Camera { get; set; }

        public BundleAdjuster(CameraPose[]? cameraPosePredictions = null, Point[]? pointPredictions = null)
        {


            CameraPosePrediction = cameraPosePredictions;
            PointPrediction = pointPredictions;

            Camera = new EmpCamera(0, 0);
        }

        public void UpdatePredictions()
        {

        }


    }
}