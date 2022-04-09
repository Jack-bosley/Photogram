using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Core.Rendering.ErrorHandling;

namespace Core.Rendering.Entities
{
    public abstract class Camera<T> : IDisposable
        where T : CameraRenderArgs
    {
        protected Texture? texture;
        protected ComputeShader? computeShader;

        protected int resolutionWidth;
        protected int resolutionHeight;

        protected float nearPointDistance;
        protected float farPointDistance;
        protected float fov;

        protected Transform transform;
        private Transform previousRenderTransform;

        public delegate void PreRenderHandler();
        public PreRenderHandler? OnPreRender;

        public delegate void RenderHandler(T args);
        public RenderHandler? OnRender;

        protected Camera(int width, int height, string computeShaderSource)
        {
            resolutionWidth = width;
            resolutionHeight = height;
            InitializeShader(computeShaderSource);

            nearPointDistance = 0;
            farPointDistance = 10000;
            fov = MathF.PI / 2;
        }
        ~Camera()
        {
            Dispose();
        }
        public void Dispose()
        {
            computeShader?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void InitializeShader(string source)
        {
            computeShader = new ComputeShader();
            computeShader.Open(source);
            computeShader.Compile();

            OpenTKException.ThrowIfErrors();
        }


        public bool IsMoved => transform.position != previousRenderTransform.position || transform.rotation != previousRenderTransform.rotation;


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

        public void RenderView(T args)
        {
            OnPreRender?.Invoke();
            OnRender?.Invoke(args);

            previousRenderTransform = transform;
        }
    }

    public class CameraPreRenderArgs : EventArgs
    {
        public bool isMoved;
    }
    public class CameraRenderArgs : EventArgs
    {
        
    }
}
