using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Core.Rendering.ErrorHandling;

namespace Core.Rendering.Entities.Rays
{
    /// <summary>
    /// Sphere-only raytraced renderer for point cloud rendering
    /// </summary>
    public class RayCamera : Camera<RayCameraRenderArgs>
    {
        protected ComputeShader? computeShader;

        int raySSBO;
        private Ray[]? rayBundle;

        public RayCamera(int width, int height) : base(width, height)
        {
            InitializeRays();
            InitializeShader();

            OnPreRender += UpdateRayPositions;
            OnRender += RenderView;
        }
        ~RayCamera()
        {
            Dispose();
        }
        public new void Dispose()
        {
            computeShader?.Dispose();
            base.Dispose();
        }
        private void InitializeShader()
        {
            computeShader = new ComputeShader();
            computeShader.Open(Properties.Resources.Raytrace_comp);
            computeShader.Compile();

            OpenTKException.ThrowIfErrors();
        }

        private void InitializeRays()
        {
            raySSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, raySSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, resolutionWidth * resolutionHeight * Ray.SIZE, rayBundle, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            rayBundle = new Ray[resolutionWidth * resolutionHeight];
            for (int x = 0; x < resolutionWidth; x++)
                for (int y = 0; y < resolutionHeight; y++)
                    rayBundle[x + (y * resolutionWidth)] = new Ray();

            UpdateRayPositions();

            OpenTKException.ThrowIfErrors();
        }


        private void UpdateRayPositions()
        {
            if (!IsMoved)
                return;

            if (rayBundle == null)
                InitializeRays();

            Vector3 directionVector = transform.GetDirectionVector();

            float distanceToCentre = 1 / MathF.Tan(fov / 2); 
            Vector3 centreOfView = transform.position + (distanceToCentre * directionVector);

            Vector3 horizontal = Vector3.Cross(directionVector, new Vector3(0, 1, 0)).Normalized();
            Vector3 vertical = -Vector3.Cross(directionVector, horizontal).Normalized() * resolutionHeight / resolutionWidth;

            for (int x = 0; x < resolutionWidth; x++)
            {
                for (int y = 0; y < resolutionHeight; y++)
                {
                    int i = x + (y * resolutionWidth);

                    float tx = (2 * x / (float)(resolutionWidth - 1)) - 1;
                    float ty = (2 * y / (float)(resolutionHeight - 1)) - 1;
                    Vector3 targetPoint = centreOfView - (tx * horizontal) - (ty * vertical);
                    Vector3 direction = (targetPoint - transform.position).Normalized();

                    rayBundle![i].origin = new Vector4(transform.position);
                    rayBundle![i].direction = new Vector4(direction);
                    rayBundle![i].pixel = new Vector2i(x, y);

                    rayBundle![i].nearPointCutoff = nearPointDistance;
                    rayBundle![i].farPointCutoff = farPointDistance;
                    rayBundle![i].isReflection = false;
                }
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, raySSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, resolutionWidth * resolutionHeight * Ray.SIZE, rayBundle, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }
        private new void RenderView(RayCameraRenderArgs args)
        {
            // Check that an output texture is bound
            if (texture == null)
                return;

            // Init shader if not already
            if (computeShader == null)
                return;


            texture.Bind(TextureUnit.Texture0);
            GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);

            computeShader!.UseProgram();
            GL.Uniform1(GL.GetUniformLocation(computeShader, "u_width"), resolutionWidth);
            GL.Uniform1(GL.GetUniformLocation(computeShader, "u_height"), resolutionHeight);
            GL.Uniform1(GL.GetUniformLocation(computeShader, "u_sphere_count"), args.spheresCount);

            int raysblockIndex = GL.GetProgramResourceIndex(computeShader, ProgramInterface.ShaderStorageBlock, "rays_ssbo");
            GL.ShaderStorageBlockBinding(computeShader, raysblockIndex, 1);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, raySSBO);

            int spheresblockIndex = GL.GetProgramResourceIndex(computeShader, ProgramInterface.ShaderStorageBlock, "spheres_ssbo");
            GL.ShaderStorageBlockBinding(computeShader, spheresblockIndex, 2);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, args.spheresSSBO);

            int materialsblockIndex = GL.GetProgramResourceIndex(computeShader, ProgramInterface.ShaderStorageBlock, "materials_ssbo");
            GL.ShaderStorageBlockBinding(computeShader, materialsblockIndex, 3);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, args.materialsSSBO);

            OpenTKException.ThrowIfErrors();

            GL.DispatchCompute(resolutionWidth / 1, resolutionHeight / 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
    }

    public class RayCameraRenderArgs : CameraRenderArgs
    {
        public int spheresSSBO;
        public int materialsSSBO;
        public int spheresCount;
    }
}
