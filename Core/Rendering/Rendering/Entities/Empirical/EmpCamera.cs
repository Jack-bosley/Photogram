using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Core.Rendering.ErrorHandling;

namespace Core.Rendering.Entities.Empirical
{
    /// <summary>
    /// Acts as a point primitive only vertex shader, with empirical camera adjustments
    /// </summary>
    public class EmpCamera : Camera<EmpCameraRenderArgs>
    {
        protected ComputeShader? projectionShader;
        protected ComputeShader? displayShader;

        // x, y focal lengths
        public Vector2 focalLength;

        // r^0, r^1, r^2 order radial distortion coefficients
        public (float r0, float r1, float r2) radialDistortionCoefficient;
        // x, y tangential distortions
        public Vector2 tangentialDistortionCoefficient;

        public Matrix3 cameraRotationMatrix;

        public EmpCamera(int width, int height) : base(width, height)
        {
            InitializeShaders();

            focalLength = new Vector2(1, 1);
            radialDistortionCoefficient = (0, 0, 0);
            tangentialDistortionCoefficient = new Vector2(0, 0);

            OnPreRender += GetRotationMatrix;
            OnRender += RenderView;
        }
        ~EmpCamera()
        {
            Dispose();
        }
        public new void Dispose()
        {
            base.Dispose();
        }
        private void InitializeShaders()
        {
            projectionShader = new ComputeShader();
            projectionShader.Open(Properties.Resources.EmpiricalProjection_comp);
            projectionShader.Compile();

            OpenTKException.ThrowIfErrors();


            displayShader = new ComputeShader();
            displayShader.Open(Properties.Resources.EmpiricalProjectionDisplay_comp);
            displayShader.Compile();

            OpenTKException.ThrowIfErrors();
        }

        private void GetRotationMatrix()
        {
            if (!IsMoved)
                return;

            cameraRotationMatrix = transform.GetRotationMatrix();
        }

        private new void RenderView(EmpCameraRenderArgs args)
        {
            // Check that an output texture is bound
            if (texture == null)
                return;

            // Init shader if not already
            if (projectionShader == null || displayShader == null)
                return;

            projectionShader!.UseProgram();
            GL.Uniform2(GL.GetUniformLocation(projectionShader, "u_focal_lengths"), focalLength);
            GL.Uniform3(GL.GetUniformLocation(projectionShader, "u_radial_distortion"), radialDistortionCoefficient);
            GL.Uniform2(GL.GetUniformLocation(projectionShader, "u_tangential_distortion"), tangentialDistortionCoefficient);
            GL.UniformMatrix3(GL.GetUniformLocation(projectionShader, "u_camera_rotation"), false, ref cameraRotationMatrix);
            GL.Uniform3(GL.GetUniformLocation(projectionShader, "u_camera_position"), transform.position);
            GL.Uniform2(GL.GetUniformLocation(projectionShader, "u_output_resolution"), resolutionWidth, resolutionHeight);

            int pointsBlockIndex = GL.GetProgramResourceIndex(projectionShader, ProgramInterface.ShaderStorageBlock, "points_ssbo");
            GL.ShaderStorageBlockBinding(projectionShader, pointsBlockIndex, 1);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, args.pointsSSBO);

            OpenTKException.ThrowIfErrors();

            GL.DispatchCompute(args.pointsCount / 1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            displayShader!.UseProgram();
            texture.Bind(TextureUnit.Texture0);
            GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);

            int displayPointsBlockIndex = GL.GetProgramResourceIndex(displayShader, ProgramInterface.ShaderStorageBlock, "points_ssbo");
            GL.ShaderStorageBlockBinding(displayShader, displayPointsBlockIndex, 1);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, args.pointsSSBO);

            GL.DispatchCompute(args.pointsCount / 1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
    }

    public class EmpCameraRenderArgs : CameraRenderArgs
    {
        public int pointsSSBO;
        public int pointsCount;
    }
}
