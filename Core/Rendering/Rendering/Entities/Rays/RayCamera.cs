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
    public class RayCamera : IDisposable
    {
        private int resolutionWidth;
        private int resolutionHeight;
        private float nearPointDistance = 0;
        private float farPointDistance = 10000;

        private Texture? texture;

        public Transform transform;
        public float fov;

        int raySSBO;
        private Ray[] rayBundle;

        ComputeShader computeShader;

        bool isCameraMoved = true;


        public RayCamera(int width, int height)
        {
            resolutionWidth = width;
            resolutionHeight = height;

            transform = new Transform();
            fov = MathF.PI / 2;

            rayBundle = new Ray[resolutionWidth * resolutionHeight];
            for (int x = 0; x < resolutionWidth; x++)
                for (int y = 0; y < resolutionHeight; y++)
                    rayBundle[x + (y * resolutionWidth)] = new Ray();

            computeShader = new ComputeShader();
            computeShader.Open(Properties.Resources.Raytrace_comp);
            computeShader.Compile();

            raySSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, raySSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, resolutionWidth * resolutionHeight * Ray.SIZE, rayBundle, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            OpenTKException.ThrowIfErrors();
        }
        ~RayCamera()
        {
            Dispose();
        }
        public void Dispose()
        {
            computeShader.Dispose();
        }

        public void BindToPanel(DisplayPanel panel)
        {
            texture = panel.viewTex;
            texture.LoadTexture(resolutionWidth, resolutionHeight);
            texture.InvalidateBuffers();
            texture.Update(true);
        }

        public void BindToTexture(Texture outputTexture)
        {
            texture = outputTexture;
            texture.LoadTexture(resolutionWidth, resolutionHeight);
            texture.InvalidateBuffers();
            texture.Update(true);
        }

        public void UpdateRayPositions()
        {
            float distanceToCentre = 1 / MathF.Tan(fov / 2); 
            Vector3 centreOfView = transform.position + (distanceToCentre * transform.rotation);

            Vector3 horizontal = Vector3.Cross(transform.rotation, new Vector3(0, 1, 0)).Normalized();
            Vector3 vertical = -Vector3.Cross(transform.rotation, horizontal).Normalized() * resolutionHeight / resolutionWidth;

            for (int x = 0; x < resolutionWidth; x++)
            {
                for (int y = 0; y < resolutionHeight; y++)
                {
                    int i = x + (y * resolutionWidth);

                    float tx = (2 * x / (float)(resolutionWidth - 1)) - 1;
                    float ty = (2 * y / (float)(resolutionHeight - 1)) - 1;
                    Vector3 targetPoint = centreOfView + (tx * horizontal) + (ty * vertical);
                    Vector3 direction = (targetPoint - transform.position).Normalized();

                    rayBundle[i].origin = new Vector4(transform.position);
                    rayBundle[i].direction = new Vector4(direction);
                    rayBundle[i].pixel = new Vector2i(x, y);

                    rayBundle[i].nearPointCutoff = nearPointDistance;
                    rayBundle[i].farPointCutoff = farPointDistance;
                    rayBundle[i].isReflection = false;
                }
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, raySSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, resolutionWidth * resolutionHeight * Ray.SIZE, rayBundle, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            isCameraMoved = false;
        }

        public void RenderView(int spheresSSBO, int materialsSSBO, int spheresCount)
        {
            // Check that an output texture is bound
            if (texture == null)
                return;

            if (isCameraMoved)
                UpdateRayPositions();

            texture.Bind(TextureUnit.Texture0);
            GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);

            computeShader.UseProgram();
            GL.Uniform1(GL.GetUniformLocation(computeShader, "u_width"), resolutionWidth);
            GL.Uniform1(GL.GetUniformLocation(computeShader, "u_height"), resolutionHeight);
            GL.Uniform1(GL.GetUniformLocation(computeShader, "u_sphere_count"), spheresCount);

            int raysblockIndex = GL.GetProgramResourceIndex(computeShader, ProgramInterface.ShaderStorageBlock, "rays_ssbo");
            GL.ShaderStorageBlockBinding(computeShader, raysblockIndex, 1);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, raySSBO);

            int spheresblockIndex = GL.GetProgramResourceIndex(computeShader, ProgramInterface.ShaderStorageBlock, "spheres_ssbo");
            GL.ShaderStorageBlockBinding(computeShader, spheresblockIndex, 2);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, spheresSSBO);

            int materialsblockIndex = GL.GetProgramResourceIndex(computeShader, ProgramInterface.ShaderStorageBlock, "materials_ssbo");
            GL.ShaderStorageBlockBinding(computeShader, materialsblockIndex, 3);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, materialsSSBO);


            OpenTKException.ThrowIfErrors();

            GL.DispatchCompute(resolutionWidth / 1, resolutionHeight / 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
    }
}
