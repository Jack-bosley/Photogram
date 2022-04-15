using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    public interface ICameraData
    {
        public Vector2i Resolution { get; set; }
        public float FOV { get; set; }
        public Matrix3 RotationMatrix { get; set; }
    }
}
