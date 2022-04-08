using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace Core.Rendering.Entities
{
    /// <summary>
    /// Quad for displaying a texture
    /// </summary>
    public class DisplayPanel
    {
        private int viewQuadVBO;
        private int viewQuadIBO;
        private int viewQuadVAO;

        public Material viewMat;
        public Texture viewTex;

        public DisplayPanel()
        {
            // Quad
            float[] viewQuad = new float[]
            {
                -1f, -1f, 0, 0, 0,
                -1f,  1f, 0, 0, 1,
                 1f, -1f, 0, 1, 0,
                 1f,  1f, 0, 1, 1,
            };
            uint[] viewQuadIndices = new uint[]
            {
                0, 1, 2,
                1, 2, 3,
            };

            viewQuadVAO = GL.GenVertexArray();
            viewQuadVBO = GL.GenBuffer();
            viewQuadIBO = GL.GenBuffer();

            GL.BindVertexArray(viewQuadVAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, viewQuadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, viewQuad.Length * sizeof(float), viewQuad, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, viewQuadIBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, viewQuadIndices.Length * sizeof(uint), viewQuadIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // Texture
            viewTex = new Texture();
            viewTex.LoadTexture(8, 8);
            viewTex.Update(true);

            // Material
            viewMat = new Material();
            viewMat.Shader = new Shader();
            viewMat.Shader.Open(Properties.Resources.default_vert, ShaderType.VertexShader);
            viewMat.Shader.Open(Properties.Resources.default_frag, ShaderType.FragmentShader);
            viewMat.Shader.Compile();
        }
        ~DisplayPanel()
        {
            Dispose();
        }
        public void Dispose()
        {
            GL.DeleteBuffer(viewQuadVBO);
            GL.DeleteBuffer(viewQuadIBO);
        }

        public void Draw()
        {
            viewMat.Bind();
            viewMat.SetTexture("texture0", TextureUnit.Texture0, viewTex);

            GL.BindVertexArray(viewQuadVAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, viewQuadIBO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
