using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelSharp;

public class Shader
{
    readonly int _handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexShaderSource = File.ReadAllText(vertexPath);

        string fragmentShaderSource = File.ReadAllText(fragmentPath);

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);


        GL.CompileShader(vertexShader);

        int success;

        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(fragmentShader);

        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }

        _handle = GL.CreateProgram();

        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);

        GL.LinkProgram(_handle);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(_handle);
            Console.WriteLine(infoLog);
        }


        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }

    public void Use()
    {
        GL.UseProgram(_handle);
    }


    private bool _disposedValue = false;

    public Shader(int handle)
    {
        _handle = handle;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        GL.DeleteProgram(_handle);

        _disposedValue = true;
    }

    ~Shader()
    {
        if (_disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(_handle, attribName);
    }

    public void SetModelMatrix(Matrix4 getModelMatrix)
    {
        throw new NotImplementedException();
    }
}