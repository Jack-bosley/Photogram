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
    public unsafe class EmpCamera : Camera<EmpCameraRenderArgs, EmpCameraData>
    {
        protected ComputeShader? clearTextureShader;
        protected ComputeShader? projectionShader;
        protected ComputeShader? displayShader;

        public Matrix3 cameraRotationMatrix;

        public EmpCamera(int width, int height) : base()
        {
            InitializeShaders();

            cameraData.Resolution = new Vector2i(width, height);
            cameraData.FOV = MathF.PI / 2;
            cameraData.FocalLength = new Vector2(1, 1);
            cameraData.RadialDistortionCoefficient = (0, 0, 0);
            cameraData.TangentialDistortionCoefficient = new Vector2(0, 0);

            OnPreRender += GetRotationMatrix;
            OnRender += RenderView;
        }
        public EmpCamera(EmpCameraData empCameraData) : base()
        {
            InitializeShaders();

            cameraData = empCameraData;

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
            clearTextureShader = new ComputeShader();
            clearTextureShader.Open(Properties.Resources.clear_texture_comp);
            clearTextureShader.Compile();
            OpenTKException.ThrowIfErrors();

            projectionShader = new ComputeShader();
            projectionShader.Open(Properties.Resources.empirical_projection_comp);
            projectionShader.Compile();
            OpenTKException.ThrowIfErrors();

            displayShader = new ComputeShader();
            displayShader.Open(Properties.Resources.empirical_projection_display_comp);
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
            // Check if an output texture is wanted
            bool renderToTexture = texture != null;

            // Init shader if not already
            if (projectionShader == null || displayShader == null)
                return;

            if (renderToTexture)
            {
                texture!.Bind(TextureUnit.Texture0);
                GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);

                // Clear the texture
                clearTextureShader!.UseProgram();
                GL.Uniform4(GL.GetUniformLocation(projectionShader, "u_clear_colour"), 0, 0, 0, 0);
                OpenTKException.ThrowIfErrors();

                GL.DispatchCompute((cameraData.Resolution.X / 32) + 1, (cameraData.Resolution.Y / 32) + 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                OpenTKException.ThrowIfErrors();
            }

            // Project the points into the camera plane
            projectionShader!.UseProgram();
            GL.Uniform2(GL.GetUniformLocation(projectionShader, "u_focal_lengths"), cameraData.FocalLength);
            GL.Uniform3(GL.GetUniformLocation(projectionShader, "u_radial_distortion"), cameraData.RadialDistortionCoefficient);
            GL.Uniform2(GL.GetUniformLocation(projectionShader, "u_tangential_distortion"), cameraData.TangentialDistortionCoefficient);
            GL.UniformMatrix3(GL.GetUniformLocation(projectionShader, "u_camera_rotation"), false, ref cameraRotationMatrix);
            GL.Uniform3(GL.GetUniformLocation(projectionShader, "u_camera_position"), transform.position);
            GL.Uniform2(GL.GetUniformLocation(projectionShader, "u_output_resolution"), cameraData.Resolution.X, cameraData.Resolution.Y);

            int worldPointsBlockIndex = GL.GetProgramResourceIndex(projectionShader, ProgramInterface.ShaderStorageBlock, "world_points_ssbo");
            GL.ShaderStorageBlockBinding(projectionShader, worldPointsBlockIndex, 1);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, args.worldPointsSSBO);

            int screenPointsBlockIndex = GL.GetProgramResourceIndex(projectionShader, ProgramInterface.ShaderStorageBlock, "screen_points_ssbo");
            GL.ShaderStorageBlockBinding(projectionShader, screenPointsBlockIndex, 2);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, args.screenPointsSSBO);

            OpenTKException.ThrowIfErrors();

            GL.DispatchCompute(args.pointsCount / 1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            OpenTKException.ThrowIfErrors();

            if (renderToTexture)
            {
                // Draw points to texture for debugging
                displayShader!.UseProgram();
                int displayScreenPointsBlockIndex = GL.GetProgramResourceIndex(displayShader, ProgramInterface.ShaderStorageBlock, "screen_points_ssbo");
                GL.ShaderStorageBlockBinding(displayShader, displayScreenPointsBlockIndex, 2);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, args.screenPointsSSBO);
                OpenTKException.ThrowIfErrors();

                GL.DispatchCompute(args.pointsCount / 1, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                OpenTKException.ThrowIfErrors();
            }
        }

        public void Resize(int width, int height)
        {
            cameraData.Resolution = new Vector2i(width, height);

            if (texture != null)
            {
                texture.LoadTexture(width, height);
                texture.InvalidateBuffers();
                texture.Update(true);
            }
        }
    }

    public class EmpCameraRenderArgs : CameraRenderArgs
    {
        public int pointsCount;

        public int worldPointsSSBO;
        public int screenPointsSSBO;
    }
}
