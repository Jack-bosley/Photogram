using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using Core.Rendering.Entities;
using Core.Rendering.Entities.Empirical;
using Core.Photogrammetry;

namespace Application
{
    public unsafe class Scene
    {
        public DisplayPanel displayPanel;
        public EmpCamera camera;

        public LabelledPoint[] worldPoints;
        public int worldPointsSSBO;

        public ScreenPoint[] screenPoints;
        public int screenPointsSSBO;

        bool isRendered = false;

        public Scene()
        {
            displayPanel = new DisplayPanel();

            camera = new EmpCamera(1920, 1080);
            camera.cameraData.FocalLength = new Vector2(1, 16 / 9.0f);
            camera.cameraData.RadialDistortionCoefficient = (0, 0f, 0f);

            camera.BindToPanel(displayPanel);

            worldPointsSSBO = GL.GenBuffer();
            screenPointsSSBO = GL.GenBuffer();

            worldPoints = new LabelledPoint[3];
            worldPoints[0] = new LabelledPoint() { pointID = 0, position = new Vector3(   0,    0, 0)};
            worldPoints[1] = new LabelledPoint() { pointID = 1, position = new Vector3(0.1f,    0, 0)};
            worldPoints[2] = new LabelledPoint() { pointID = 2, position = new Vector3(   0, 0.1f, 0)};

            screenPoints = new ScreenPoint[3];
            screenPoints[0] = new ScreenPoint();
            screenPoints[1] = new ScreenPoint();
            screenPoints[2] = new ScreenPoint();

        }

        public (List<Frame> frames, LabelledPoint[] startingGuess) GetDummyData()
        {
            Console.WriteLine("Started: Create Dummy Frames");

            int numFrames = 100;
            int numPoints = 1000;
            (float min, float max) xRange = (-0.5f, 0.5f);
            (float min, float max) yRange = (-0.5f, 0.5f);
            (float min, float max) zRange = (-0.5f, 0.5f);

            float cameraOrbitRadius = 5;

            // Set camera positions for dummy frames
            DateTime transformsStart = DateTime.Now;

            Transform[] dummyCameraTransforms = new Transform[numFrames];
            for (int i = 0; i < numFrames; i++)
            {
                float t = (float)i / (numFrames - 1) * 2 * MathF.PI;

                dummyCameraTransforms[i] = new Transform()
                {
                    position = new Vector3(MathF.Sin(t), 0, MathF.Cos(t)) * cameraOrbitRadius,
                    rotation = new Vector3(0, MathF.PI + t, 0),
                };
            }

            Console.WriteLine($"\tComplete: Create Dummy Transforms {(DateTime.Now - transformsStart).TotalSeconds}");


            // Set points to view in camera
            DateTime pointsStart = DateTime.Now;

            Random random = new Random();
            static float Map(float a, (float min, float max) range) => (a * range.min) + ((1 - a) * range.max);

            LabelledPoint[] dummyPoints = new LabelledPoint[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                float xRand = random.NextSingle();
                float yRand = random.NextSingle();
                float zRand = random.NextSingle();

                dummyPoints[i] = new LabelledPoint()
                {
                    pointID = i,
                    position = new Vector3(Map(xRand, xRange), Map(yRand, yRange), Map(zRand, zRange)),
                };
            }

            Console.WriteLine($"\tComplete: Create Dummy Points {(DateTime.Now - pointsStart).TotalSeconds}");


            // Generate the frames and a starting guess from the dummy data 
            DateTime imagesStart = DateTime.Now;

            List<Frame> frames = BundleAdjuster.GenerateDummyData(camera, dummyCameraTransforms, dummyPoints, true);
            LabelledPoint[] startingGuess = BundleAdjuster.CreateDummyGuesses(frames);

            Console.WriteLine($"\tComplete: Create Dummy Frames + Starting Guess {(DateTime.Now - imagesStart).TotalSeconds}");

            
            return (frames, startingGuess);
        }

        public void PerformBundleAdjustment()
        {
            (List<Frame> frames, LabelledPoint[] startingGuess) dummyData = GetDummyData();

            // Create a bundle adjuster for the dummy data
            BundleAdjuster bundleAdjuster = new BundleAdjuster();
            bundleAdjuster.AddFrames(dummyData.frames);
            bundleAdjuster.SetStartingGuess(dummyData.startingGuess);


            bundleAdjuster.GenerateBuffers();


            isRendered = true;
        }


        public void DrawMainCamera()
        {
            if (!isRendered)
                PerformBundleAdjustment();

            displayPanel.Draw();
        }

        private EmpCameraRenderArgs BufferSceneData()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, worldPointsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, worldPoints.Length * sizeof(LabelledPoint), worldPoints, BufferUsageHint.DynamicDraw);

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
