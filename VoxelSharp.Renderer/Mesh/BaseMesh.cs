using System.Buffers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer.Mesh;

public abstract class BaseMesh : IDisposable, IRenderable
{
    private int _vao;
    private int _vbo;
    protected int VertexCount;

    protected bool IsInitialized => _vao != 0 && _vbo != 0;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Render(Shader shaderProgram)
    {
        if (_vbo == 0 || _vao == 0 || VertexCount == 0) return;

        // Bind the VAO
        GL.BindVertexArray(_vao);

        // Issue OpenGL draw call
        GL.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);

        // Unbind the VAO
        GL.BindVertexArray(0);
    }

    protected abstract void SetVertexAttributes(Shader shaderProgram);

    protected virtual void SetupMesh(int elementsPerVertex, Shader shaderProgram)
    {
        if (_vao != 0)
        {
            GL.DeleteVertexArray(_vao);
            _vao = 0;
        }

        // Get vertex data using Memory<float> to minimize heap allocations
        using var vertexMemoryOwner = GetVertexDataMemory(out var vertexCount);

        if (vertexCount == 0)
        {
            // No data, skip setup
            VertexCount = 0;
            return;
        }

        VertexCount = vertexCount / elementsPerVertex;

        // Generate VAO and VBO
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

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

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.Identity;
    }

    // Use Memory<T> for better control over data and memory allocation
    protected abstract IMemoryOwner<float> GetVertexDataMemory(out int vertexCount);

    // Proper disposal to avoid resource leaks
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (_vao != 0)
        {
            GL.DeleteVertexArray(_vao);
            _vao = 0;
        }

        if (_vbo != 0)
        {
            GL.DeleteBuffer(_vbo);
            _vbo = 0;
        }
    }

    ~BaseMesh()
    {
        Dispose(false);
    }
}