using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelSharp.Renderer;

public class Shader
{
    private readonly Dictionary<string, int> _attributeLocations;

    private readonly Dictionary<string, int> _uniformLocations;
    private readonly int _handle;

    public Shader(string vertPath, string fragPath)
    {
        if (!File.Exists(vertPath) || !File.Exists(fragPath)) throw new FileNotFoundException("Shader file not found.");

        // Load and compile shaders
        var vertexShader = LoadAndCompileShader(vertPath, ShaderType.VertexShader);
        var fragmentShader = LoadAndCompileShader(fragPath, ShaderType.FragmentShader);

        // Create shader program and link shaders
        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);
        LinkProgram(_handle);

        // Clean up individual shaders
        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // cache attribute locations
        GL.GetProgram(_handle, GetProgramParameterName.ActiveAttributes, out var numberOfAttributes);
        _attributeLocations = new Dictionary<string, int>();

        for (var i = 0; i < numberOfAttributes; i++)
        {
            var key = GL.GetActiveAttrib(_handle, i, out _, out _);
            var location = GL.GetAttribLocation(_handle, key);

            if (location == -1) Console.WriteLine($"Warning: Attribute '{key}' is not active in the shader.");

            _attributeLocations[key] = location;
        }

        // Cache uniform locations
        GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
        _uniformLocations = new Dictionary<string, int>();

        for (var i = 0; i < numberOfUniforms; i++)
        {
            var key = GL.GetActiveUniform(_handle, i, out _, out _);
            var location = GL.GetUniformLocation(_handle, key);

            if (location == -1) Console.WriteLine($"Warning: Uniform '{key}' is not active in the shader.");

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
        GL.UseProgram(_handle);
    }

    public static void UnUse()
    {
        GL.UseProgram(0);
    }

    public int GetAttribLocation(string attribName)
    {
        if (_attributeLocations.TryGetValue(attribName, out var location) && location != -1) return location;

        Console.WriteLine($"Warning: Attribute '{attribName}' not found in shader.");
        return -1;
    }

    // Uniform setters
    public void SetUniform(string name, int data)
    {
        if (_uniformLocations.TryGetValue(name, out var location) && location != -1) GL.Uniform1(location, data);
    }

    public void SetUniform(string name, float data)
    {
        if (_uniformLocations.TryGetValue(name, out var location) && location != -1) GL.Uniform1(location, data);
    }

    public void SetUniform(string name, Matrix4 data)
    {
        if (_uniformLocations.TryGetValue(name, out var location) && location != -1)
            GL.UniformMatrix4(location, true, ref data);
    }

    public void SetUniform(string name, Vector3 data)
    {
        if (_uniformLocations.TryGetValue(name, out var location) && location != -1) GL.Uniform3(location, ref data);
    }

    ~Shader()
    {
        GL.DeleteProgram(_handle);
    }
}