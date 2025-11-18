using System.Buffers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer.Mesh;

public abstract class BaseMesh : IDisposable, IRenderable
{
    // Opaque mesh data
    protected int _opaqueVao;
    protected int _opaqueVbo;
    protected int OpaqueVertexCount;
    protected bool IsOpaqueInitialized => _opaqueVao != 0 && _opaqueVbo != 0;

    // Transparent mesh data
    protected int _transparentVao;
    protected int _transparentVbo;
    protected int TransparentVertexCount;
    protected bool IsTransparentInitialized => _transparentVao != 0 && _transparentVbo != 0;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Render(Shader shaderProgram)
    {
        RenderOpaque(shaderProgram);
        RenderTransparent(shaderProgram);
    }

    public virtual void RenderOpaque(Shader shaderProgram)
    {
        if (_opaqueVbo == 0 || _opaqueVao == 0 || OpaqueVertexCount == 0) return;

        GL.BindVertexArray(_opaqueVao);

        GL.DrawArrays(PrimitiveType.Triangles, 0, OpaqueVertexCount);

        GL.BindVertexArray(0);
    }
    
    public virtual void RenderTransparent(Shader shaderProgram)
    {
        if (_transparentVbo == 0 || _transparentVao == 0 || TransparentVertexCount == 0) return;

        GL.BindVertexArray(_transparentVao);

        GL.DrawArrays(PrimitiveType.Triangles, 0, TransparentVertexCount);

        GL.BindVertexArray(0);
    }

    protected abstract void SetVertexAttributes(Shader shaderProgram);

    protected virtual void SetupOpaqueMesh(int elementsPerVertex, Shader shaderProgram)
    {
        if (_opaqueVao != 0)
        {
            GL.DeleteVertexArray(_opaqueVao);
            _opaqueVao = 0;
        }

        // Get vertex data using Memory<float> to minimize heap allocations
        using var vertexMemoryOwner = GetOpaqueVertexDataMemory(out var vertexCount);

        if (vertexCount == 0)
        {
            // No data, skip setup
            OpaqueVertexCount = 0;
            return;
        }

        OpaqueVertexCount = vertexCount / elementsPerVertex;

        // Generate VAO and VBO
        _opaqueVao = GL.GenVertexArray();
        GL.BindVertexArray(_opaqueVao);

        _opaqueVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _opaqueVbo);

        // Bind vertex data from Memory<T>
        var vertexSpan = vertexMemoryOwner.Memory.Span[..vertexCount];
        GL.BufferData(BufferTarget.ArrayBuffer, vertexSpan.Length * sizeof(float), ref vertexSpan[0],
            BufferUsageHint.StaticDraw);

        // Set vertex attributes
        SetVertexAttributes(shaderProgram);

        // Unbind VAO and VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }
    
    
    protected virtual void SetupTransparentMesh(int elementsPerVertex, Shader shaderProgram)
    {
        if (_transparentVao != 0)
        {
            GL.DeleteVertexArray(_transparentVao);
            _transparentVao = 0;
        }

        using var vertexMemoryOwner = GetTransparentVertexDataMemory(out var vertexCount);

        if (vertexCount == 0)
        {
            TransparentVertexCount = 0;
            return;
        }

        TransparentVertexCount = vertexCount / elementsPerVertex;

        _transparentVao = GL.GenVertexArray();
        GL.BindVertexArray(_transparentVao);

        _transparentVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _transparentVbo);

        var vertexSpan = vertexMemoryOwner.Memory.Span[..vertexCount];
        GL.BufferData(BufferTarget.ArrayBuffer, vertexSpan.Length * sizeof(float), ref vertexSpan[0],
            BufferUsageHint.StaticDraw);

        SetVertexAttributes(shaderProgram);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.Identity;
    }

    protected abstract IMemoryOwner<float> GetOpaqueVertexDataMemory(out int vertexCount);
    protected abstract IMemoryOwner<float> GetTransparentVertexDataMemory(out int vertexCount);

    // Proper disposal to avoid resource leaks
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        
        // Dispose opaque buffers
        if (_opaqueVao != 0)
        {
            GL.DeleteVertexArray(_opaqueVao);
            _opaqueVao = 0;
        }
        if (_opaqueVbo != 0)
        {
            GL.DeleteBuffer(_opaqueVbo);
            _opaqueVbo = 0;
        }
        
        // Dispose transparent buffers
        if (_transparentVao != 0)
        {
            GL.DeleteVertexArray(_transparentVao);
            _transparentVao = 0;
        }
        if (_transparentVbo != 0)
        {
            GL.DeleteBuffer(_transparentVbo);
            _transparentVbo = 0;
        }
    }

    ~BaseMesh()
    {
        Dispose(false);
    }
}