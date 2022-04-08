using System;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace Application
{
    public static class Application
    {
        public static ApplicationWindow? window;

        public static void Main(string[] args)
        {
            GameWindowSettings gameWindowSettings = new GameWindowSettings()
            {
                UpdateFrequency = double.MaxValue,
            };
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                WindowState = WindowState.Normal,
                StartFocused = true,
                Size = new Vector2i(1920, 1080),
                APIVersion = Version.Parse("4.3.0"),
                Title = "RayTrace",
                Profile = ContextProfile.Compatability,
            };

            window = new ApplicationWindow(gameWindowSettings, nativeWindowSettings);
            window.Run();
        }
    }

}