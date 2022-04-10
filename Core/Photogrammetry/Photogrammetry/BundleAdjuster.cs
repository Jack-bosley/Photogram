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

        public BundleAdjuster()
        {
            Frames = new List<Frame>();
            PointGuesses = new LabelledPoint[0];
        }

        public void AddFrames(IEnumerable<Frame> frame) => Frames.AddRange(frame);
        public void AddFrames(params Frame[] frame) => Frames.AddRange(frame);

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

                result[i] = new Frame(camera.Resolution, screenPoints);

                if (provideCameraOrientations)
                    result[i].CameraOrientation = camera.transform;
            }

            return result;
        }

        public void CreateDummyGuesses()
        {
            // Enumerate over all points, getting each ID uniquely
            HashSet<int> pointIdSet = new HashSet<int>();
            foreach (Frame frame in Frames)
            {
                foreach (int pointId in frame.FramePointImage.GetPointIDs())
                    pointIdSet.Add(pointId);
            }

            int i = 0;
            PointGuesses = new LabelledPoint[pointIdSet.Count];
            foreach (int pointId in pointIdSet)
                PointGuesses[i++] = new LabelledPoint() { pointID = pointId, };
        }

    }
}