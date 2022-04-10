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

        protected int resolutionWidth;
        protected int resolutionHeight;

        protected float nearPointDistance;
        protected float farPointDistance;
        protected float fov;

        public Transform transform;
        private Transform? previousRenderTransform;

        public delegate void PreRenderHandler();
        public PreRenderHandler? OnPreRender;

        public delegate void RenderHandler(T args);
        public RenderHandler? OnRender;

        protected Camera(int width, int height)
        {
            transform = new Transform();
            previousRenderTransform = null;

            resolutionWidth = width;
            resolutionHeight = height;

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
            GC.SuppressFinalize(this);
        }



        public bool IsMoved => previousRenderTransform == null || transform.position != previousRenderTransform?.position || transform.rotation != previousRenderTransform?.rotation;
        public Vector2i Resolution => new Vector2i(resolutionWidth, resolutionHeight);

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
