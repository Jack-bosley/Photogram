using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Core.Rendering.Entities
{
    public struct Transform
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Transform()
        {
            position = new Vector3(0, 0, 0);
            rotation = new Vector3(1, 0, 0);
            scale = new Vector3(1, 1, 1);
        }
    }
}
