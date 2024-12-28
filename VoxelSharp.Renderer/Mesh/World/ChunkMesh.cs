using System.Buffers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;

namespace VoxelSharp.Renderer.Mesh.World
{
    public class ChunkMesh(Chunk chunk) : BaseMesh
    {
        public override void Render(Shader shaderProgram)
        {
            // Check if the chunk is dirty or uninitialized
            if (chunk.IsDirty || !IsInitialized)
            {
                // Rebuild the mesh (8 elements per vertex)
                SetupMesh(8, shaderProgram);
                chunk.IsDirty = false;
            }

            // After setup, skip rendering if there are no vertices
            if (VertexCount == 0) return;

            // Set the model matrix for this chunk
            shaderProgram.SetUniform("m_model", GetModelMatrix());

            // Call the base Render to handle VAO binding and OpenGL draw calls
            base.Render(shaderProgram);
        }

        /// <summary>
        /// Uses a memory pool to rent a float buffer to store vertex data.
        /// Once completed, SetupMesh will store this data in a GPU buffer,
        /// and this memory will be returned to the pool.
        /// </summary>
        /// <param name="vertexCount">Returns the total float elements used.</param>
        /// <returns>An IMemoryOwner of float, which you can dispose or return to the pool.</returns>
        protected override IMemoryOwner<float> GetVertexDataMemory(out int vertexCount)
        {
            // Estimate the required size for the vertex buffer:
            //   ChunkVolume * 6 faces * 6 vertices/face * 8 float elements/vertex
            int estimatedVertexCount = chunk.ChunkVolume * 6 * 6 * 8;
            IMemoryOwner<float> memoryOwner = MemoryPool<float>.Shared.Rent(estimatedVertexCount);

            // Get a span from the rented memory
            Span<float> span = memoryOwner.Memory.Span;

            var index = 0;

            // Retrieve a span of the chunk's voxel data
            Span<Voxel> chunkVoxelSpan = chunk.VoxelBuffer.Span;

            for (var x = 0; x < chunk.ChunkSize; x++)
            for (var z = 0; z < chunk.ChunkSize; z++)
            for (var y = 0; y < chunk.ChunkSize; y++)
            {
                // Compute this voxel's index
                var voxelIndex = chunk.GetVoxelIndex(new Position<int>(x, y, z));
                var voxel = chunkVoxelSpan[voxelIndex];

                // Skip transparent voxels
                if (voxel.Color.A == 0) continue;

                // Add visible faces
                AddVisibleFacesToSpan(span, chunkVoxelSpan, ref index, x, y, z, voxel);
            }

            vertexCount = index; // The total floats used in the span
            return memoryOwner;
        }

        /// <summary>
        /// Sets up vertex attribute pointers for this mesh.
        /// </summary>
        /// <param name="shaderProgram">Active shader program to query attributes from.</param>
        protected override void SetVertexAttributes(Shader shaderProgram)
        {
            // Position attribute
            var posIndex = shaderProgram.GetAttribLocation("in_position");
            if (posIndex != -1)
            {
                GL.EnableVertexAttribArray(posIndex);
                GL.VertexAttribPointer(posIndex, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                    IntPtr.Zero);
            }

            // Color attribute
            var colorIndex = shaderProgram.GetAttribLocation("in_color");
            if (colorIndex != -1)
            {
                GL.EnableVertexAttribArray(colorIndex);
                GL.VertexAttribPointer(colorIndex, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                    (IntPtr)(3 * sizeof(float)));
            }

            // Face ID attribute
            var faceIndex = shaderProgram.GetAttribLocation("face_id");
            if (faceIndex == -1) return;
            GL.EnableVertexAttribArray(faceIndex);
            GL.VertexAttribPointer(faceIndex, 1, VertexAttribPointerType.Float, false, 8 * sizeof(float),
                (IntPtr)(7 * sizeof(float)));
        }

        /// <summary>
        /// Computes the model matrix for this chunk based on its world position.
        /// </summary>
        /// <returns>A translation matrix placing this chunk in world space.</returns>
        public override Matrix4 GetModelMatrix()
        {
            // Calculate the translation for this chunk
            return Matrix4.CreateTranslation(
                chunk.Position.X * chunk.ChunkSize,
                chunk.Position.Y * chunk.ChunkSize,
                chunk.Position.Z * chunk.ChunkSize
            );
        }

        /// <summary>
        /// Checks whether the voxel at the specified coordinates is "void" from the perspective of rendering
        /// (i.e., out of bounds or transparent).
        /// </summary>
        /// <param name="x">X coordinate within the chunk.</param>
        /// <param name="y">Y coordinate within the chunk.</param>
        /// <param name="z">Z coordinate within the chunk.</param>
        /// <param name="currentAlpha">Alpha value of the current voxel.</param>
        /// <param name="voxelSpan">Span of all voxels in this chunk.</param>
        private bool IsVoid(int x, int y, int z, int currentAlpha, Span<Voxel> voxelSpan)
        {
            // If the current voxel is transparent, treat as void
            if (currentAlpha == 0) return true;

            // If out of chunk bounds, treat as void
            if (!IsWithinBounds(x, y, z)) return true;

            // Otherwise, check if the adjacent voxel is transparent
            int idx = chunk.GetVoxelIndex(new Position<int>(x, y, z));
            return voxelSpan[idx].Color.A != currentAlpha;
        }

        private bool IsWithinBounds(int x, int y, int z)
        {
            return (x >= 0 && x < chunk.ChunkSize) &&
                   (y >= 0 && y < chunk.ChunkSize) &&
                   (z >= 0 && z < chunk.ChunkSize);
        }

        /// <summary>
        /// For a given voxel, checks each face to determine if it should be rendered.
        /// If visible, adds the corresponding vertices to the shared vertex span.
        /// </summary>
        /// <param name="span">The vertex buffer span.</param>
        /// <param name="voxelSpan">Span of this chunk's voxels.</param>
        /// <param name="index">Reference to the current index in the vertex buffer span.</param>
        /// <param name="x">Voxel X coordinate.</param>
        /// <param name="y">Voxel Y coordinate.</param>
        /// <param name="z">Voxel Z coordinate.</param>
        /// <param name="voxel">The voxel being processed.</param>
        private void AddVisibleFacesToSpan(Span<float> span, Span<Voxel> voxelSpan, ref int index,
            int x, int y, int z, Voxel voxel)
        {
            int alpha = voxel.Color.A;

            // Top face
            if (IsVoid(x, y + 1, z, alpha, voxelSpan))
            {
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Top));
            }

            // Bottom face
            if (IsVoid(x, y - 1, z, alpha, voxelSpan))
            {
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Bottom));
            }

            // Right face
            if (IsVoid(x + 1, y, z, alpha, voxelSpan))
            {
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Right));
            }

            // Left face
            if (IsVoid(x - 1, y, z, alpha, voxelSpan))
            {
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Left));
            }

            // Back face
            if (IsVoid(x, y, z - 1, alpha, voxelSpan))
            {
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Back));
            }

            // Front face
            if (IsVoid(x, y, z + 1, alpha, voxelSpan))
            {
                AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Front));
            }
        }

        /// <summary>
        /// Adds each vertex to the shared vertex buffer span.
        /// </summary>
        /// <param name="span">The float span for our vertex data.</param>
        /// <param name="index">A reference to the current write position in the span.</param>
        /// <param name="vertices">A collection of VoxelVertex structs that will be written into the span.</param>
        private static void AddVerticesToSpan(Span<float> span, ref int index, IEnumerable<VoxelVertex> vertices)
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