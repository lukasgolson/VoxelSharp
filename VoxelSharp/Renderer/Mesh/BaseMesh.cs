using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelSharp.Renderer.Mesh;

public abstract class BaseMesh : IDisposable
{
    protected int Vao = 0;
    protected int Vbo = 0;
    protected int VertexCount = 0;

    protected abstract void SetVertexAttributes();

    protected virtual void SetupMesh(int elementsPerVertex)
    {
        // Check if VAO exists and delete it
        if (Vao != 0)
        {
            GL.DeleteVertexArray(Vao);
        }

        var vertexData = GetVertexData();
        VertexCount = vertexData.Count / elementsPerVertex; // Render this based on your vertex structure

        // Generate and bind Vertex Array Object (VAO)
        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        // Generate and bind Vertex Buffer Object (VBO)
        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(float), vertexData.ToArray(),
            BufferUsageHint.StaticDraw);

        SetVertexAttributes();

        // Unbind VAO and VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public virtual void Dispose()
    {
        // Clean up the VAO and VBO when the mesh is destroyed
        if (Vao != 0) GL.DeleteVertexArray(Vao);
        if (Vbo != 0) GL.DeleteBuffer(Vbo);
    }

    public virtual void Render(Shader shaderProgram)
    {
        // Bind the Vertex Array Object (VAO) for rendering
        GL.BindVertexArray(Vao);

        // Issue OpenGL draw calls to render the mesh
        GL.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);

        // Unbind the VAO after rendering
        GL.BindVertexArray(0);
    }

    public abstract List<float> GetVertexData(); // Pure abstract function

    public virtual Matrix4 GetModelMatrix()
    {
        return Matrix4.Identity;
    }
}