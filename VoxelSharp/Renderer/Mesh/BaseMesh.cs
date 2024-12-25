using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Buffers;

namespace VoxelSharp.Renderer.Mesh
{
    public abstract class BaseMesh : IDisposable
    {
        protected int Vao = 0;
        protected int Vbo = 0;
        protected int VertexCount = 0;

        protected abstract void SetVertexAttributes();

        protected virtual void SetupMesh(int elementsPerVertex)
        {
            if (Vao != 0)
            {
                GL.DeleteVertexArray(Vao);
                Vao = 0;
            }

            // Get vertex data using Memory<float> to minimize heap allocations
            using var vertexMemoryOwner = GetVertexDataMemory(out int vertexCount);
            VertexCount = vertexCount / elementsPerVertex;

            // Generate VAO and VBO
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);

            // Bind vertex data from Memory<T>
            var vertexSpan = vertexMemoryOwner.Memory.Span.Slice(0, vertexCount);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexSpan.Length * sizeof(float), ref vertexSpan[0], BufferUsageHint.StaticDraw);

            // Set vertex attributes
            SetVertexAttributes();

            // Unbind VAO and VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public virtual void Render(Shader shaderProgram)
        {
            // Bind the VAO
            GL.BindVertexArray(Vao);

            // Issue OpenGL draw call
            GL.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);

            // Unbind the VAO
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
            if (disposing)
            {
                if (Vao != 0)
                {
                    GL.DeleteVertexArray(Vao);
                    Vao = 0;
                }
                if (Vbo != 0)
                {
                    GL.DeleteBuffer(Vbo);
                    Vbo = 0;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseMesh()
        {
            Dispose(false);
        }

        protected bool IsInitialized => Vao != 0 && Vbo != 0;
    }
}
