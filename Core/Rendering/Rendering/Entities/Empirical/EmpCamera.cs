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
    internal class EmpCamera : Camera<EmpCameraRenderArgs>
    {
        public EmpCamera(int width, int height) : base(width, height, Properties.Resources.EmpiricalProjection_comp)
        {
            transform = new Transform();
            InitializeShader();

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

        private void InitializeShader()
        {
            computeShader = new ComputeShader();
            computeShader.Open(Properties.Resources.Raytrace_comp);
            computeShader.Compile();

            OpenTKException.ThrowIfErrors();
        }

        private void RenderView(EmpCameraRenderArgs args)
        {
            // Check that an output texture is bound
            if (texture == null)
                return;

            // Init shader if not already
            if (computeShader == null)
                InitializeShader();


            texture.Bind(TextureUnit.Texture0);
            GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);

            computeShader!.UseProgram();

            OpenTKException.ThrowIfErrors();

            GL.DispatchCompute(resolutionWidth / 1, resolutionHeight / 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
    }

    public class EmpCameraRenderArgs : CameraRenderArgs
    {
        public int pointsSSBO;
    }
}
