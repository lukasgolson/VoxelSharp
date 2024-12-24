using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelSharp
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            // Load the shader source from the file.
            var shaderSource = File.ReadAllText(vertPath);
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // We can now detach and delete the shaders, as they are no longer needed.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Now, we can get the locations of the uniforms in the shader and cache them.

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                var location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code == (int)All.True) return;

            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred while compiling Shader({shader}).\n\n{infoLog}");
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
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
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Internal method to handle common uniform-setting operations.
        /// </summary>
        private void SetUniformInternal(string name, out int location)
        {
            GL.UseProgram(Handle);

            if (_uniformLocations.TryGetValue(name, out location))
            {
                Console.WriteLine($"Setting uniform {name}");
            }
            else
            {
                Console.WriteLine($"Uniform {name} not found in shader.");
                location = -1; // Indicate that the uniform was not found
            }
        }

        /// <summary>
        /// Sets an int uniform variable.
        /// </summary>
        public void SetUniform(string name, int data)
        {
            SetUniformInternal(name, out var location);
            if (location != -1)
            {
                GL.Uniform1(location, data);
            }
        }

        /// <summary>
        /// Sets a float uniform variable.
        /// </summary>
        public void SetUniform(string name, float data)
        {
            SetUniformInternal(name, out var location);
            if (location != -1)
            {
                GL.Uniform1(location, data);
            }
        }

        /// <summary>
        /// Sets a Matrix4 uniform variable.
        /// </summary>
        public void SetUniform(string name, Matrix4 data)
        {
            SetUniformInternal(name, out var location);
            if (location != -1)
            {
                GL.UniformMatrix4(location, true, ref data);
            }
        }

        /// <summary>
        /// Sets a Vector3 uniform variable.
        /// </summary>
        public void SetUniform(string name, Vector3 data)
        {
            SetUniformInternal(name, out var location);
            if (location != -1)
            {
                GL.Uniform3(location, ref data);
            }
        }
    }
}