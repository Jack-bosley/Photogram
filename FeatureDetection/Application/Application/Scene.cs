using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using Core.Rendering.Entities;
using Core.Rendering.Entities.Rays;

namespace Application
{
    public class Scene
    {
        public DisplayPanel displayPanel;

        public Scene()
        {
            displayPanel = new DisplayPanel();
        }


        public void DrawMainCamera()
        {
            displayPanel.Draw();
        }


        private void BufferSceneData()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }
    }
}
