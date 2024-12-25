using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Structs;
using VoxelSharp.World;
using System;
using System.Buffers;

namespace VoxelSharp.Renderer.Mesh.World
{
    public class ChunkMesh : BaseMesh
    {
        private readonly Chunk _chunk;

        public ChunkMesh(Chunk chunk)
        {
            _chunk = chunk;
        }

        public override void Render(Shader shaderProgram)
        {
            // Bind the model matrix to the shader program
            shaderProgram.SetUniform("m_model", GetModelMatrix());

            // Check if the chunk is dirty or mesh is uninitialized
            if (_chunk.IsDirty || !IsInitialized)
            {
                SetupMesh(8); // 8 elements per vertex: position (3), color (4), face ID (1)
                _chunk.IsDirty = false;
            }

            // Call the base render to handle VAO binding and drawing
            base.Render(shaderProgram);
        }

        protected override IMemoryOwner<float> GetVertexDataMemory(out int vertexCount)
        {
            // Estimate the required size for the vertex buffer
            int estimatedVertexCount = _chunk.ChunkVolume * 6 * 6 * 8; // Max possible vertices: 6 faces per voxel, 6 vertices per face, 8 elements per vertex
            IMemoryOwner<float> memoryOwner = MemoryPool<float>.Shared.Rent(estimatedVertexCount);

            var span = memoryOwner.Memory.Span;
            int index = 0;

            for (int x = 0; x < _chunk.ChunkSize; ++x)
            {
                for (int z = 0; z < _chunk.ChunkSize; ++z)
                {
                    for (int y = 0; y < _chunk.ChunkSize; ++y)
                    {
                        Voxel voxel = _chunk.Voxels[_chunk.GetVoxelIndex(new Position<int>(x, y, z))];

                        // Skip transparent voxels
                        if (voxel.Color.A == 0) continue;

                        // Add visible faces
                        AddVisibleFacesToSpan(span, ref index, x, y, z, voxel);
                    }
                }
            }

            vertexCount = index; // Total populated elements in the span
            return memoryOwner;
        }

        protected override void SetVertexAttributes()
        {
            // Position attribute (location = 0)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), IntPtr.Zero);

            // Color attribute (location = 1)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                (IntPtr)(3 * sizeof(float)));

            // Face ID attribute (location = 2)
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                (IntPtr)(7 * sizeof(float)));
        }

        public override Matrix4 GetModelMatrix()
        {
            // Calculate the translation for this chunk
            return Matrix4.CreateTranslation(
                _chunk.Position.X * _chunk.ChunkSize,
                _chunk.Position.Y * _chunk.ChunkSize,
                _chunk.Position.Z * _chunk.ChunkSize
            );
        }

        private bool IsVoid(int x, int y, int z, int currentAlpha, Voxel[] voxels)
        {
            if (!IsWithinBounds(x, y, z)) return true;

            return voxels[_chunk.GetVoxelIndex(new Position<int>(x, y, z))].Color.A != currentAlpha;
        }

        private bool IsWithinBounds(int x, int y, int z)
        {
            return x >= 0 && x < _chunk.ChunkSize &&
                   y >= 0 && y < _chunk.ChunkSize &&
                   z >= 0 && z < _chunk.ChunkSize;
        }

        private void AddVisibleFacesToSpan(Span<float> span, ref int index, int x, int y, int z, Voxel voxel)
        {
            // Check each face and add it if it's visible
            if (IsVoid(x, y + 1, z, voxel.Color.A, _chunk.Voxels)) // Top face
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Top));

            if (IsVoid(x, y - 1, z, voxel.Color.A, _chunk.Voxels)) // Bottom face
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Bottom));

            if (IsVoid(x + 1, y, z, voxel.Color.A, _chunk.Voxels)) // Right face
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Right));

            if (IsVoid(x - 1, y, z, voxel.Color.A, _chunk.Voxels)) // Left face
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Left));

            if (IsVoid(x, y, z - 1, voxel.Color.A, _chunk.Voxels)) // Back face
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Back));

            if (IsVoid(x, y, z + 1, voxel.Color.A, _chunk.Voxels)) // Front face
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Front));
        }

        private void AddVerticesToSpan(Span<float> span, ref int index, IEnumerable<VoxelVertex> vertices)
        {
            foreach (var vertex in vertices)
            {
                span[index++] = vertex.X;
                span[index++] = vertex.Y;
                span[index++] = vertex.Z;
                span[index++] = vertex.R;
                span[index++] = vertex.G;
                span[index++] = vertex.B;
                span[index++] = vertex.A;
                span[index++] = vertex.FaceId;
            }
        }
    }
}
