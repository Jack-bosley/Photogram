using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using OpenTK.Graphics.OpenGL;

namespace Core.Rendering.Entities
{
    public class ComputeShader : IDisposable
    {
        private readonly int shaderProgram;

        private readonly int shader;

        private bool isDisposed = false;
        private bool isCompiled = false;

        #region Constructor
        public ComputeShader()
        {
            shader = GL.CreateShader(ShaderType.ComputeShader);

            shaderProgram = GL.CreateProgram();
        }

        ~ComputeShader()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            GL.DeleteShader(shader);
            GL.DeleteProgram(shaderProgram);

            isDisposed = true;
        }
        #endregion

        public int ShaderID => shaderProgram;

        public void Open(string source)
        {
            GL.ShaderSource(shader, source);
        }

        public void Compile()
        {
            GL.CompileShader(shader);
            string infoLogVert = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrEmpty(infoLogVert))
                throw new InvalidOperationException($"Compute shader compilation failed:\n{infoLogVert}");

            GL.AttachShader(shaderProgram, shader);
            GL.LinkProgram(shaderProgram);
            GL.DetachShader(shaderProgram, shader);

            isCompiled = true;
        }

        public void UseProgram()
        {
            if (isDisposed)
                throw new ObjectDisposedException("Shader program is already disposed");

            if (!isCompiled)
                throw new InvalidOperationException("Shader must be compiled before use");

            GL.UseProgram(this);
        }


        public static implicit operator int(ComputeShader shader) => shader.shaderProgram;
    }
}