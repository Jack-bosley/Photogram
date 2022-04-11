using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Configuration;

using OpenTK.Graphics.OpenGL;
using System.Collections.Specialized;

namespace Core.Rendering.Entities
{
    public class Shader : IDisposable
    {
        private static readonly string? directory;
        private static readonly NameValueCollection? shaderNameValueCollection;

        private static readonly Shader defaultShader;


        private readonly int shaderProgram;

        private readonly int vertexShader;
        private readonly int fragmentShader;

        private bool isDisposed = false;
        private bool isCompiled = false;

        public Dictionary<ShaderType, string?> shaderNames;


        #region Constructor
        static Shader()
        {
            directory = ConfigurationManager.AppSettings["shader_directory"];
            if (directory == null)
                throw new ArgumentNullException("directory", "shader_directory is not defined in App.config");

            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException("directory", "shader_directory is empty in App.config");
                        
            shaderNameValueCollection = ConfigurationManager.GetSection("shaders") as NameValueCollection;
            if (shaderNameValueCollection == null)
                throw new ArgumentNullException("shaderNameValueCollection", "No section with tags <shaders> found in App.config");

            defaultShader = new Shader();
            defaultShader.Open("default_vert", ShaderType.VertexShader);
            defaultShader.Open("default_frag", ShaderType.FragmentShader);
            defaultShader.Compile();
        }
        public Shader()
        {
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            shaderProgram = GL.CreateProgram();

            shaderNames = new Dictionary<ShaderType, string?>();
        }

        ~Shader()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.DeleteProgram(shaderProgram);

            isDisposed = true;
        }
        #endregion

        public static Shader Default => defaultShader;
        public int ShaderID => shaderProgram;

        public void Open(string name, ShaderType shaderType)
        {
            string source = File.ReadAllText(GetShaderDirectory(name));
            switch (shaderType)
            {
                case ShaderType.VertexShader: GL.ShaderSource(vertexShader, source); break;
                case ShaderType.FragmentShader: GL.ShaderSource(fragmentShader, source); break;
            }
            shaderNames[shaderType] = directory;
        }

        public void Compile()
        {
            GL.CompileShader(vertexShader);
            string infoLogVert = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrEmpty(infoLogVert))
                throw new InvalidOperationException($"Vertex shader compilation failed:\n{infoLogVert}");

            GL.CompileShader(fragmentShader);
            string infoLogFrag = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrEmpty(infoLogFrag))
                throw new InvalidOperationException($"Fragment shader compilation failed:\n{infoLogFrag}");

            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            GL.LinkProgram(shaderProgram);

            GL.DetachShader(shaderProgram, vertexShader);
            GL.DetachShader(shaderProgram, fragmentShader);

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


        public static implicit operator int(Shader shader) => shader.shaderProgram;

        public static string GetShaderDirectory(string name)
        {
            string? shaderName = shaderNameValueCollection![name];
            if (shaderName == null)
                throw new ArgumentNullException("shader", $"shader with name {name} is not defined in App.config");

            string fullPath = Path.Combine(directory!, shaderName);

            if (!File.Exists(fullPath))
                throw new ArgumentException("directory", $"No shader found with path {fullPath}");

            return fullPath;
        }
    }
}