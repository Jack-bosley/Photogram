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
        public int cameraViewSSBO;

        BundleAdjuster bundleAdjuster;

        bool isRendered = false;



        public Scene()
        {
            displayPanel = new DisplayPanel();

            bundleAdjuster = new BundleAdjuster();

            camera = new EmpCamera(1920, 1080);
            camera.cameraData.FocalLength = new Vector2(1, 16 / 9.0f);
            camera.cameraData.RadialDistortionCoefficient = (0, 0f, 0f);

            camera.BindToPanel(displayPanel);
        }

        public (List<Frame> frames, LabelledPoint[] startingGuess) GetDummyData()
        {
            Console.WriteLine("Started: Create Dummy Frames");

            int numFrames = 400;
            int numPoints = 5000;
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
            bundleAdjuster.AddFrames(dummyData.frames);
            bundleAdjuster.SetStartingGuess(dummyData.startingGuess);

            bundleAdjuster.GenerateBuffers();

            DateTime adjustStart = DateTime.Now;
            bundleAdjuster.Adjust();
            Console.WriteLine($"\tComplete: Adjustment (1 iteration) {(DateTime.Now - adjustStart).TotalSeconds}");

            isRendered = true;
        }


        public unsafe void DrawMainCamera()
        {
            if (!isRendered)
            {
                PerformBundleAdjustment();

                cameraViewSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, cameraViewSSBO);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, bundleAdjuster.PointCount * sizeof(ScreenPoint), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }

            EmpCameraRenderArgs args = new EmpCameraRenderArgs()
            {
                pointsCount = bundleAdjuster.PointCount,

                worldPointsSSBO = bundleAdjuster.PointGuessesSSBO,
                screenPointsSSBO = cameraViewSSBO,

                renderToTexture = true,
            };

            camera.SetCameraData(bundleAdjuster.GetCameraData(0));
            camera.SetCameraTransform(bundleAdjuster.GetCameraTransformation(0));
            camera.RenderView(args);

            displayPanel.Draw();
        }
    }
}
