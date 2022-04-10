using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using Core.Rendering.Entities.Empirical;
using Core.Rendering.Entities;



namespace Core.Photogrammetry
{
    public class BundleAdjuster
    {
        private List<Frame> Frames { get; set; }
        private LabelledPoint[] PointGuesses { get; set; }

        private int PointGuessesSSBO;


        public BundleAdjuster()
        {
            Frames = new List<Frame>();
            PointGuesses = new LabelledPoint[0];
        }


        public void AddFrames(IEnumerable<Frame> frame) => Frames.AddRange(frame);
        public void AddFrames(params Frame[] frame) => Frames.AddRange(frame);

        public void SetStartingGuess(LabelledPoint[] startingGuess) => PointGuesses = startingGuess;


        public unsafe void GenerateBuffers()
        {
            PointGuessesSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, PointGuessesSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, PointGuesses.Length * sizeof(LabelledPoint), PointGuesses, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            foreach (Frame frame in Frames)
                frame.GenerateBuffers();
        }

        public void GetReprojectionError()
        {
            EmpCameraRenderArgs args = new EmpCameraRenderArgs()
            {
                pointsCount = PointGuesses.Length,
                worldPointsSSBO = PointGuessesSSBO,
            };

            EmpCamera camera = new EmpCamera(0, 0);
            foreach (Frame frame in Frames)
            {
                // Set frame specific data
                camera.SetCameraData(frame.CameraData);
                args.screenPointsSSBO = frame.ImagePointSSBO;

                // 
                camera.RenderView(args);


            }
        }


        public void Adjust()
        {
            EmpCamera camera = new EmpCamera(0, 0);

        }


        public unsafe static List<Frame> GenerateDummyData(EmpCamera camera, Transform[] cameraPositions, LabelledPoint[] truePointPositions, bool provideCameraOrientations = false)
        {
            int worldPointsSSBO = GL.GenBuffer();
            int screenPointsSSBO = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, worldPointsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, truePointPositions.Length * sizeof(LabelledPoint), truePointPositions, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, screenPointsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, truePointPositions.Length * sizeof(ScreenPoint), IntPtr.Zero, BufferUsageHint.DynamicRead);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            EmpCameraRenderArgs args = new EmpCameraRenderArgs()
            {
                pointsCount = truePointPositions.Length,

                worldPointsSSBO = worldPointsSSBO,
                screenPointsSSBO = screenPointsSSBO,
            };

            ScreenPoint[] screenPoints = new ScreenPoint[truePointPositions.Length];
            List<Frame> result = new List<Frame>(new Frame[cameraPositions.Length]);
            for (int i = 0; i < cameraPositions.Length; i++)
            {
                camera.transform = cameraPositions[i];
                camera.RenderView(args);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, args.screenPointsSSBO);
                GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, args.pointsCount * sizeof(ScreenPoint), screenPoints);

                result[i] = new Frame(camera.cameraData.Resolution, screenPoints);
                result[i].CameraData = camera.cameraData;

                if (provideCameraOrientations)
                    result[i].CameraTransform = camera.transform;
            }

            return result;
        }
        public static LabelledPoint[] CreateDummyGuesses(List<Frame> frames)
        {
            // Enumerate over all points, getting each ID uniquely
            HashSet<int> pointIdSet = new HashSet<int>();
            foreach (Frame frame in frames)
            {
                foreach (int pointId in frame.FramePointImage.GetPointIDs())
                    pointIdSet.Add(pointId);
            }

            int i = 0;
            LabelledPoint[] pointGuesses = new LabelledPoint[pointIdSet.Count];
            foreach (int pointId in pointIdSet)
                pointGuesses[i++] = new LabelledPoint() { pointID = pointId, };

            return pointGuesses;
        }
    }
}