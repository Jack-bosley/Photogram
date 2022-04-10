using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities.Rays
{
    public struct RayCameraData : ICameraData
    {
        public Vector2i Resolution { get; set; }
        public float FOV { get; set; }

        public float NearPointCutoffDistance { get; set; }
        public float FarPointCutoffDistance { get; set; }
    }
}
