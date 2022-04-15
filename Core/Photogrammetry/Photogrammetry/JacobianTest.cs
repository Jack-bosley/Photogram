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
    public class JacobianTest
    {
        private ComputeShader Jacobians { get; set; }

        public JacobianTest()
        {

            Jacobians = new ComputeShader();
            Jacobians.Open("jacobians_comp");
            Jacobians.Compile();
        }

        public unsafe void Test(Transform cameraTransform, EmpCameraData cameraData, LabelledPoint testPointTransform)
        {
            EmpCamera camera = new EmpCamera(cameraData);
            camera.SetCameraTransform(cameraTransform);

            // Project the test point to get the true screen point
            ScreenPoint[] truePoints = ProjectPoints(camera, new LabelledPoint[] { testPointTransform });

            // Offset point and camera by some known amount
            Vector3 dE = new Vector3(0, 0, 0);
            Vector3 dC = new Vector3(0, 0, 0);
            Vector3 dX = new Vector3(0, 0, 0);

            Transform offsetCameraTransform = new Transform()
            {
                position = cameraTransform.position + dC,
                rotation = cameraTransform.rotation + dE,
                scale = cameraTransform.scale
            };

            LabelledPoint offsetPoint = new LabelledPoint()
            {
                pointID = testPointTransform.pointID,
                position = testPointTransform.position + dX,
            };

            // Project offset point
            camera.SetCameraTransform(offsetCameraTransform);
            ScreenPoint[] offsetPoints = ProjectPoints(camera, new LabelledPoint[] { offsetPoint });


            ScreenPointError[] errors = ComputeErrors(truePoints, offsetPoints);
            Jacobian[] jacobians = ComputeJacobian(cameraTransform, cameraData, new LabelledPoint[] { testPointTransform });
        }

        private unsafe ScreenPoint[] ProjectPoints(EmpCamera camera, LabelledPoint[] testPoints)
        {
            int originalScreenPointSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, originalScreenPointSSBO);
            GL.BufferData(BufferTarget.CopyWriteBuffer, testPoints.Length * sizeof(ScreenPoint), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            int originalWorldPointSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, originalWorldPointSSBO);
            GL.BufferData(BufferTarget.CopyWriteBuffer, testPoints.Length * sizeof(LabelledPoint), testPoints, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);

            EmpCameraRenderArgs args = new EmpCameraRenderArgs()
            {
                pointsCount = 1,

                screenPointsSSBO = originalScreenPointSSBO,
                worldPointsSSBO = originalWorldPointSSBO,
            };

            camera.RenderView(args);

            ScreenPoint[] screenPoints = new ScreenPoint[testPoints.Length];
            GL.BindBuffer(BufferTarget.CopyReadBuffer, originalScreenPointSSBO);
            GL.GetBufferSubData(BufferTarget.CopyReadBuffer, IntPtr.Zero, testPoints.Length * sizeof(ScreenPoint), screenPoints);

            GL.BindBuffer(BufferTarget.CopyReadBuffer, 0);

            return screenPoints;
        }

        private unsafe ScreenPointError[] ComputeErrors(ScreenPoint[] truePoints, ScreenPoint[] offsetPoints)
        {
            ScreenPointError[] errors = new ScreenPointError[truePoints.Length];

            for (int i = 0; i < truePoints.Length; i++)
            {
                Vector2 r = truePoints[i].pixelPosition - offsetPoints[i].pixelPosition;
                errors[i] = new ScreenPointError()
                {
                    errorVector = r,
                    errorRadius = MathF.Sqrt(Vector2.Dot(r, r)),
                };
            }

            return errors;
        }

        private unsafe Jacobian[] ComputeJacobian(Transform cameraTransform, EmpCameraData cameraData, LabelledPoint[] testPoints)
        {
            int cameraPositionsSSBO;
            int cameraRotationsSSBO;
            int cameraDatasSSBO;
            int cameraRotationDerivativesSSBO;
            int pointGuessesSSBO;

            int jacobiansSSBO;


            // Copy camera + test points position and rotation into buffers
            {
                Vector3[] cameraPositions = new Vector3[] { cameraTransform.position };
                Vector3[] cameraRotations = new Vector3[] { cameraTransform.rotation };
                EmpCameraData[] cameraDatas = new EmpCameraData[] { cameraData };
                (Matrix3 x, Matrix3 y, Matrix3 z)[] cameraRotationDerivatives = new (Matrix3 x, Matrix3 y, Matrix3 z)[]
                {
                    cameraTransform.GetRotationEulerVectorDerivative(),
                };

                cameraPositionsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraPositionsSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, 1 * sizeof(Vector3), cameraPositions, BufferUsageHint.DynamicCopy);

                cameraRotationsSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraRotationsSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, 1 * sizeof(Vector3), cameraRotations, BufferUsageHint.DynamicCopy);

                cameraDatasSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraDatasSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, 1 * sizeof(EmpCameraData), cameraDatas, BufferUsageHint.DynamicCopy);

                cameraRotationDerivativesSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, cameraRotationDerivativesSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, 1 * 3 * sizeof(Matrix3), cameraRotationDerivatives, BufferUsageHint.DynamicCopy);

                pointGuessesSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, pointGuessesSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, testPoints.Length * sizeof(LabelledPoint), testPoints, BufferUsageHint.DynamicCopy);


                GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
            }

            // Calculate the jacobians
            {
                jacobiansSSBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, jacobiansSSBO);
                GL.BufferData(BufferTarget.CopyWriteBuffer, 1 * sizeof(Jacobian), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
                OpenTKException.ThrowIfErrors();

                Jacobians.UseProgram();
                GL.Uniform1(GL.GetUniformLocation(Jacobians, "u_point_count"), testPoints.Length);

                int pointPositionsBlockIndex = GL.GetProgramResourceIndex(Jacobians, ProgramInterface.ShaderStorageBlock, "point_positions_ssbo");
                GL.ShaderStorageBlockBinding(Jacobians, pointPositionsBlockIndex, 1);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointGuessesSSBO);
                OpenTKException.ThrowIfErrors();

                int cameraPositionsBlockIndex = GL.GetProgramResourceIndex(Jacobians, ProgramInterface.ShaderStorageBlock, "camera_positions_ssbo");
                GL.ShaderStorageBlockBinding(Jacobians, cameraPositionsBlockIndex, 2);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, cameraPositionsSSBO);
                OpenTKException.ThrowIfErrors();

                int cameraRotationsBlockIndex = GL.GetProgramResourceIndex(Jacobians, ProgramInterface.ShaderStorageBlock, "camera_rotations_ssbo");
                GL.ShaderStorageBlockBinding(Jacobians, cameraRotationsBlockIndex, 3);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, cameraRotationsSSBO);
                OpenTKException.ThrowIfErrors();

                int cameraDatasBlockIndex = GL.GetProgramResourceIndex(Jacobians, ProgramInterface.ShaderStorageBlock, "camera_datas_ssbo");
                GL.ShaderStorageBlockBinding(Jacobians, cameraDatasBlockIndex, 4);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, cameraDatasSSBO);
                OpenTKException.ThrowIfErrors();

                int jacobiansBlockIndex = GL.GetProgramResourceIndex(Jacobians, ProgramInterface.ShaderStorageBlock, "jacobians_ssbo");
                GL.ShaderStorageBlockBinding(Jacobians, jacobiansBlockIndex, 6);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, jacobiansSSBO);
                OpenTKException.ThrowIfErrors();


                GL.DispatchCompute(1, testPoints.Length, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
                OpenTKException.ThrowIfErrors();
            }

            Jacobian[] jacobian = new Jacobian[testPoints.Length];
            GL.BindBuffer(BufferTarget.CopyReadBuffer, jacobiansSSBO);
            GL.GetBufferSubData(BufferTarget.CopyReadBuffer, IntPtr.Zero, testPoints.Length * sizeof(Jacobian), jacobian);
            GL.BindBuffer(BufferTarget.CopyReadBuffer, jacobiansSSBO);

            return jacobian;
        }
    }
}
