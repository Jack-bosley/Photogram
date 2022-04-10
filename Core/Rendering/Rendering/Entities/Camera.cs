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
    public abstract class Camera<T, U> : IDisposable
        where T : CameraRenderArgs
        where U : ICameraData
    {
        protected Texture? texture;

        public U? cameraData;

        public Transform transform;
        private Transform? previousRenderTransform;

        public delegate void PreRenderHandler();
        public PreRenderHandler? OnPreRender;

        public delegate void RenderHandler(T args);
        public RenderHandler? OnRender;

        protected Camera()
        {
            transform = new Transform();
            previousRenderTransform = null;
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

        public void BindToPanel(DisplayPanel panel)
        {
            if (cameraData == null)
                return;

            texture = panel.viewTex;
            texture.LoadTexture(cameraData.Resolution.X, cameraData.Resolution.Y);
            texture.InvalidateBuffers();
            texture.Update(true);
        }

        public void BindToTexture(Texture outputTexture)
        {
            if (cameraData == null)
                return;

            texture = outputTexture;
            texture.LoadTexture(cameraData.Resolution.X, cameraData.Resolution.Y);
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
