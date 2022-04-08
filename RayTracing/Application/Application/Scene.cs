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
        public RayCamera mainCamera;
        public DisplayPanel displayPanel;

        public Sphere[] spheres;
        private int spheresSSBO;

        public RenderMaterial[] materials;
        private int materialSSBO;

        bool isSceneChanged = true;

        public Scene()
        {
            displayPanel = new DisplayPanel();

            mainCamera = new RayCamera(1920, 1080);
            mainCamera.BindToPanel(displayPanel);

            spheres = new Sphere[3];
            spheres[0] = new Sphere() { origin = new Vector4(-5, 0, -2, 0), radius = 1, material = 0, };
            spheres[1] = new Sphere() { origin = new Vector4(-5, 0, 2, 0), radius = 1, material = 1, };
            spheres[2] = new Sphere() { origin = new Vector4(5, 0, 0, 0), radius = 1, material = 2, };

            materials = new RenderMaterial[3];
            materials[0] = new RenderMaterial() { reflectivity = 0.3f, color = new Color4(1.0f, 0.5f, 0.4f, 0), };
            materials[1] = new RenderMaterial() { reflectivity = 0.4f, color = new Color4(0.4f, 1.0f, 0.4f, 0), };
            materials[2] = new RenderMaterial() { reflectivity = 0.5f, color = new Color4(0.7f, 0.1f, 1.0f, 0), };

            spheresSSBO = GL.GenBuffer();
            materialSSBO = GL.GenBuffer();
        }


        public void DrawMainCamera()
        {
            if (isSceneChanged)
                BufferSceneData();

            mainCamera.RenderView(spheresSSBO, materialSSBO, spheres.Length);

            displayPanel.Draw();

        }


        private void BufferSceneData()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, spheresSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, spheres.Length * Sphere.SIZE, spheres, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, materialSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, materials.Length * RenderMaterial.SIZE, materials, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            isSceneChanged = false;
        }
    }
}
