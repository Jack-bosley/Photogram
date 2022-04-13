using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Core.Rendering.Entities.Empirical;
using Core.Rendering.Entities;
using Core.Rendering.ErrorHandling;


namespace Core.Photogrammetry
{
    public class BundleAdjuster
    {
        private List<Frame> Frames { get; set; }

        public int PointGuessesSSBO { get; private set; }
        private LabelledPoint[] PointGuesses { get; set; }

        private ComputeShader ErrorVectors { get; set; }
        private ComputeShader Jacobians { get; set; }


        public BundleAdjuster()
        {
            Frames = new List<Frame>();
            PointGuesses = new LabelledPoint[0];

            ErrorVectors = new ComputeShader();
            ErrorVectors.Open("error_vectors_comp");
            ErrorVectors.Compile();

            Jacobians = new ComputeShader();
            Jacobians.Open("jacobians_comp");
            Jacobians.Compile();
        }

        public int PointCount => PointGuesses.Length;
        public int FrameCount => Frames.Count;

        public Transform GetCameraTransformation(int frameIndex) => Frames[frameIndex].CameraTransform;
        public EmpCameraData GetCameraData(int frameIndex) => Frames[frameIndex].CameraData;

        public void AddFrames(IEnumerable<Frame> frame) => Frames.AddRange(frame);
        public void AddFrames(params Frame[] frame) => Frames.AddRange(frame);

        public void SetStartingGuess(LabelledPoint[] startingGuess) => PointGuesses = startingGuess;


        public unsafe void GenerateBuffers()
        {
            PointGuessesSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, PointGuessesSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, PointCount * sizeof(LabelledPoint), PointGuesses, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            foreach (Frame frame in Frames)
                frame.GenerateBuffers(PointCount);
        }


        /// <summary>
        /// Optimise guesses on specific frames
        /// </summary>
        /// <param name="frameIndices">Indices of frames to consider in optimisation</param>
        public unsafe void Adjust(int[]? frameIndices = null)
        {
            // Check all frames exist
            if (frameIndices != null)
            {
                foreach (int frameIndex in frameIndices)
                {
                    if (frameIndex < 0 && frameIndex >= Frames.Count)
                        throw new Exception($"Frame with index {frameIndex} not found ({0}, {Frames.Count})");
                }
            }
            else
            {
                frameIndices = new int[FrameCount];
                for (int i = 0; i < FrameCount; i++)
                    frameIndices[i] = i;
            }

            int adjustmentFrameCount = frameIndices.Length;
            int totalAdjustmentPoints = adjustmentFrameCount * PointCount;

            int projectedPointsSSBO;
            int truePointsSSBO;
            int cameraPositionsSSBO;
            int cameraRotationsSSBO;
            int cameraDatasSSBO;

            int errorVectorsSSBO;
            int jacobiansSSBO;

            // Project all points for all frames and store in a single buffer
            {
                projectedPointsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, projectedPointsSSBO);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, totalAdjustmentPoints * sizeof(ScreenPoint), IntPtr.Zero, BufferUsageHint.DynamicDraw);

                EmpCamera camera = new EmpCamera(0, 0);
                EmpCameraRenderArgs args = new EmpCameraRenderArgs()
                {
                    pointsCount = PointCount,
                    worldPointsSSBO = PointGuessesSSBO,
                    screenPointsSSBO = projectedPointsSSBO,     // Rendering to a local buffer for later use in this method

                    ignoreMemoryBarrierBit = true,
                    renderToTexture = false,
                };

                int frameNumber = 0;
                foreach (int frameIndex in frameIndices)
                {
                    Frame frame = Frames[frameIndex];

                    camera.SetCameraData(frame.CameraData);
                    camera.SetCameraTransform(frame.CameraTransform);

                    args.screenPointsOffset = frameNumber * PointCount;

                    camera.RenderView(args);

                    frameNumber++;
                }
            }

            // Copy all true points into a single buffer
            {
                truePointsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, truePointsSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, totalAdjustmentPoints * sizeof(ScreenPoint), IntPtr.Zero, BufferUsageHint.DynamicDraw);

                IntPtr writeOffset;
                int frameSize = PointCount * sizeof(ScreenPoint);

                int frameNumber = 0;
                foreach (int frameIndex in frameIndices)
                {
                    GL.BindBuffer(BufferTarget.CopyReadBuffer, Frames[frameIndex].ImagePointSSBO);

                    writeOffset = new IntPtr(frameNumber * frameSize);
                    GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, IntPtr.Zero, writeOffset, frameSize);

                    frameNumber++;
                }
            }

            // Copy camera positions and rotations into buffers
            {
                Vector3[] cameraPositions = new Vector3[adjustmentFrameCount];
                Vector3[] cameraRotations = new Vector3[adjustmentFrameCount];
                EmpCameraData[] cameraDatas = new EmpCameraData[adjustmentFrameCount];

                int frameNumber = 0;
                foreach (int frameIndex in frameIndices)
                {
                    cameraPositions[frameNumber] = Frames[frameIndex].CameraTransform.position;
                    cameraRotations[frameNumber] = Frames[frameIndex].CameraTransform.rotation;
                    cameraDatas[frameNumber] = Frames[frameIndex].CameraData;

                    frameNumber++;
                }

                cameraPositionsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraPositionsSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, adjustmentFrameCount * sizeof(Vector3), cameraPositions, BufferUsageHint.DynamicCopy);

                cameraRotationsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraRotationsSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, adjustmentFrameCount * sizeof(Vector3), cameraRotations, BufferUsageHint.DynamicCopy);

                cameraDatasSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraDatasSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, adjustmentFrameCount * sizeof(EmpCameraData), cameraDatas, BufferUsageHint.DynamicCopy);

                GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);

            }

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            // Calculate the error vectors
            {
                errorVectorsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, errorVectorsSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, totalAdjustmentPoints * sizeof(ScreenPointError), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);

                // Compare projected to true point positions to get error vectors
                ErrorVectors.UseProgram();

                int trueScreenPointBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "true_screen_points_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, trueScreenPointBlockIndex, 1);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, truePointsSSBO);

                int projScreenPointBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "proj_screen_points_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, projScreenPointBlockIndex, 2);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, projectedPointsSSBO);

                int errorVectorsBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "error_vectors_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, errorVectorsBlockIndex, 3);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, errorVectorsSSBO);
                OpenTKException.ThrowIfErrors();

                GL.DispatchCompute(totalAdjustmentPoints, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
                OpenTKException.ThrowIfErrors();
            }

            // Calculate the jacobians
            {
                jacobiansSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, jacobiansSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, totalAdjustmentPoints * sizeof(Jacobian), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);

                Jacobians.UseProgram();

                int pointPositionsBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "point_positions_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, pointPositionsBlockIndex, 1);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, PointGuessesSSBO);

                int cameraPositionsBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "camera_positions_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, cameraPositionsBlockIndex, 2);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, cameraPositionsSSBO);

                int cameraRotationsBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "camera_rotations_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, cameraRotationsBlockIndex, 3);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, cameraRotationsSSBO);
                OpenTKException.ThrowIfErrors();

                int cameraDatasBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "camera_datas_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, cameraDatasBlockIndex, 4);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, cameraDatasSSBO);
                OpenTKException.ThrowIfErrors();

                int jacobiansBlockIndex = GL.GetProgramResourceIndex(ErrorVectors, ProgramInterface.ShaderStorageBlock, "jacobians_ssbo");
                GL.ShaderStorageBlockBinding(ErrorVectors, jacobiansBlockIndex, 6);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, jacobiansSSBO);
                OpenTKException.ThrowIfErrors();


                GL.DispatchCompute(totalAdjustmentPoints, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
                OpenTKException.ThrowIfErrors();
            }

            ScreenPoint[] truePoints = new ScreenPoint[totalAdjustmentPoints];
            ScreenPoint[] projPoints = new ScreenPoint[totalAdjustmentPoints];
            ScreenPointError[] errors = new ScreenPointError[totalAdjustmentPoints];

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, truePointsSSBO);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, totalAdjustmentPoints * sizeof(ScreenPoint), truePoints);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, projectedPointsSSBO);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, totalAdjustmentPoints * sizeof(ScreenPoint), projPoints);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, errorVectorsSSBO);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, totalAdjustmentPoints * sizeof(ScreenPointError), errors);
            
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
                camera.SetCameraTransform(cameraPositions[i]);
                camera.RenderView(args);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, args.screenPointsSSBO);
                GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, args.pointsCount * sizeof(ScreenPoint), screenPoints);

                result[i] = new Frame(camera.cameraData.Resolution, screenPoints);
                result[i].CameraData = camera.cameraData;

                if (provideCameraOrientations)
                    result[i].CameraTransform = camera.Transform;
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