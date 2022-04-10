using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;

namespace Application
{
    public class ApplicationWindow : GameWindow
    {
        public Scene? scene;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public ApplicationWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            Width = nativeWindowSettings.Size.X;
            Height = nativeWindowSettings.Size.Y;

            scene = new Scene();
        }


        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            scene!.PerformBundleAdjustment();

            base.OnLoad();
            Application.timer.StartTimer();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Width = e.Width;
            Height = e.Height;

            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Application.timer.StartFrame();
            GL.Clear(ClearBufferMask.ColorBufferBit);

            scene!.DrawMainCamera();

            SwapBuffers();
            GL.Flush();

            base.OnRenderFrame(args);
            Application.timer.EndFrame();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            Application.timer.StartUpdate();

            base.OnUpdateFrame(args);
            Application.timer.EndUpdate();
        }
    }
}
