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
        float t = 0;

        public DisplayPanel displayPanel;
        public EmpCamera camera;

        public Vector4[] worldPoints;
        public int worldPointsSSBO;

        public ScreenPoint[] screenPoints;
        public int screenPointsSSBO;

        public Scene()
        {
            displayPanel = new DisplayPanel();

            camera = new EmpCamera(1920, 1080);
            camera.focalLength.Y = 16 / 9.0f;
            camera.radialDistortionCoefficient = (0, 0f, 0f);

            camera.BindToPanel(displayPanel);

            worldPointsSSBO = GL.GenBuffer();
            screenPointsSSBO = GL.GenBuffer();

            worldPoints = new Vector4[3];
            worldPoints[0] = new Vector4(0   , 0   , 0, 0);
            worldPoints[1] = new Vector4(0.1f, -0.1f, 0, 0);
            worldPoints[2] = new Vector4(0   , 0.1f, 0, 0);

            screenPoints = new ScreenPoint[3];
            screenPoints[0] = new ScreenPoint();
            screenPoints[1] = new ScreenPoint();
            screenPoints[2] = new ScreenPoint();

        }

        public void PerformBundleAdjustment()
        {

        }

        public void DrawMainCamera()
        {
            t += Application.timer.DeltaTime;
            camera.transform.position = new Vector3(MathF.Sin(t), 0, MathF.Cos(t));
            camera.transform.rotation = new Vector3(0, MathF.PI + t, 0);

            EmpCameraRenderArgs args = BufferSceneData();
            camera.RenderView(args);

            displayPanel.Draw();
        }


        private unsafe EmpCameraRenderArgs BufferSceneData()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, worldPointsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, worldPoints.Length * sizeof(Vector4), worldPoints, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, screenPointsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, screenPoints.Length * sizeof(ScreenPoint), screenPoints, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            EmpCameraRenderArgs args = new EmpCameraRenderArgs()
            {
                pointsCount = worldPoints.Length,

                worldPointsSSBO = worldPointsSSBO,
                screenPointsSSBO = screenPointsSSBO,
            };
            return args;
        }
    }
}
