using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using Core.Rendering.Entities;

namespace Core.Photogrammetry
{
    public class Frame
    {
        public PointImage FramePointImage { get; set; }
        public Transform CameraOrientation { get; set; }

        public int ImagePointSSBO { get; private set; }

        public Frame(Vector2i frameResolution, ScreenPoint[] screenPoints)
        {
            FramePointImage = new PointImage(frameResolution, screenPoints);
        }

        public unsafe void GenerateBuffers()
        {
            ImagePointSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ImagePointSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, FramePointImage.ImagePoints.Length * sizeof(ScreenPoint), FramePointImage.ImagePoints, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }
    }
}
