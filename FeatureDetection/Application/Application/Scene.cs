using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using Core.Rendering.Entities;
using Core.Rendering.Entities.Empirical;

namespace Application
{
    public class Scene
    {
        public DisplayPanel displayPanel;
        public EmpCamera camera;

        public Point[] points;
        public int pointsSSBO;

        public Scene()
        {
            displayPanel = new DisplayPanel();

            camera = new EmpCamera(1920, 1080);
            camera.focalLength.Y = 16 / 9.0f;
            camera.radialDistortionCoefficient = (10f, 0.2f, 0.1f);
            
            camera.BindToPanel(displayPanel);

            points = new Point[3];
            points[0] = new Point() { worldPosition = new Vector4(0   , 0   , 1, 0) };
            points[1] = new Point() { worldPosition = new Vector4(0.1f, 0   , 1, 0) };
            points[2] = new Point() { worldPosition = new Vector4(0   , 0.1f, 1, 0) };

            pointsSSBO = GL.GenBuffer();
        }


        public void DrawMainCamera()
        {
            EmpCameraRenderArgs args = BufferSceneData();
            camera.RenderView(args);

            displayPanel.Draw();
        }


        private EmpCameraRenderArgs BufferSceneData()
        {
            Console.WriteLine(camera.cameraRotationMatrix);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, points.Length * Point.SIZE, points, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            EmpCameraRenderArgs args = new EmpCameraRenderArgs()
            {
                pointsSSBO = pointsSSBO,
                pointsCount = points.Length,
            };
            return args;
        }
    }
}
