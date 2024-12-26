using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;

namespace VoxelSharp.Renderer
{
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;
        private readonly Dictionary<string, int> _attributeLocations;

        public Shader(string vertPath, string fragPath)
        {
            if (!File.Exists(vertPath) || !File.Exists(fragPath))
            {
                throw new FileNotFoundException("Shader file not found.");
            }

            // Load and compile shaders
            var vertexShader = LoadAndCompileShader(vertPath, ShaderType.VertexShader);
            var fragmentShader = LoadAndCompileShader(fragPath, ShaderType.FragmentShader);

            // Create shader program and link shaders
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            LinkProgram(Handle);

            // Clean up individual shaders
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // cache attribute locations
            GL.GetProgram(Handle, GetProgramParameterName.ActiveAttributes, out var numberOfAttributes);
            _attributeLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfAttributes; i++)
            {
                var key = GL.GetActiveAttrib(Handle, i, out _, out _);
                var location = GL.GetAttribLocation(Handle, key);

                if (location == -1)
                {
                    Console.WriteLine($"Warning: Attribute '{key}' is not active in the shader.");
                }

                _attributeLocations[key] = location;
            }

            // Cache uniform locations
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);

                if (location == -1)
                {
                    Console.WriteLine($"Warning: Uniform '{key}' is not active in the shader.");
                }

                _uniformLocations[key] = location;
            }
        }

        private static int LoadAndCompileShader(string path, ShaderType type)
        {
            var shaderSource = File.ReadAllText(path);
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, shaderSource);
            CompileShader(shader);
            return shader;
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

            if (code == (int)All.True) return;

            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred while compiling Shader({shader}):\n{infoLog}");
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);

            if (code == (int)All.True) return;

            var infoLog = GL.GetProgramInfoLog(program);
            throw new Exception($"Error occurred whilst linking Program({program}):\n{infoLog}");
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public static void UnUse()
        {
            GL.UseProgram(0);
        }

        public int GetAttribLocation(string attribName)
        {
            if (_attributeLocations.TryGetValue(attribName, out var location) && location != -1) return location;
            
            Console.WriteLine($"Warning: Attribute '{attribName}' not found in shader.");
            return 0;
        }

        // Uniform setters
        public void SetUniform(string name, int data)
        {
            if (_uniformLocations.TryGetValue(name, out var location) && location != -1)
            {
                GL.Uniform1(location, data);
            }
        }

        public void SetUniform(string name, float data)
        {
            if (_uniformLocations.TryGetValue(name, out var location) && location != -1)
            {
                GL.Uniform1(location, data);
            }
        }

        public void SetUniform(string name, Matrix4 data)
        {
            if (_uniformLocations.TryGetValue(name, out var location) && location != -1)
            {
                GL.UniformMatrix4(location, true, ref data);
            }
        }

        public void SetUniform(string name, Vector3 data)
        {
            if (_uniformLocations.TryGetValue(name, out var location) && location != -1)
            {
                GL.Uniform3(location, ref data);
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }
    }
}