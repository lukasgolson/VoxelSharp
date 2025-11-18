using System.Buffers;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;

namespace VoxelSharp.Renderer.Mesh.World;

public class ChunkMesh : BaseMesh
{
    private readonly VoxelWorld _voxelWorld; // Add this

    public Chunk? Chunk { get; }

    public ChunkMesh(Chunk chunk, VoxelWorld voxelWorld) : base() // Add VoxelWorld here
    {
        Chunk = chunk;
        _voxelWorld = voxelWorld; // Store it
    }
    
    private enum MeshType {Opaque, Transparent}
    
    

    private Position<int>? Position => Chunk?.Position;

    public override void RenderOpaque(Shader shaderProgram)
    {
        if (Chunk.IsDirty || !IsOpaqueInitialized)
        {
            SetupOpaqueMesh(8, shaderProgram);
        }

        if (OpaqueVertexCount == 0) return;

        shaderProgram.SetUniform("m_model", GetModelMatrix());

        base.RenderOpaque(shaderProgram);
    }

    public override void RenderTransparent(Shader shaderProgram)
    {
        if (Chunk.IsDirty || !IsOpaqueInitialized)
        {
            SetupTransparentMesh(8, shaderProgram);
        }
        
        Chunk.IsDirty = false;


        if (TransparentVertexCount == 0) return;

        shaderProgram.SetUniform("m_model", GetModelMatrix());

        base.RenderTransparent(shaderProgram);
    }
    
    public override void Render(Shader shaderProgram)
    {
        RenderOpaque(shaderProgram);
        RenderTransparent(shaderProgram);
    }
    
    
    /// <summary>
    ///     Uses a memory pool to rent a float buffer to store vertex data.
    ///     Once completed, SetupMesh will store this data in a GPU buffer,
    ///     and this memory will be returned to the pool.
    /// </summary>
    /// <param name="vertexCount">Returns the total float elements used.</param>
    /// <returns>An IMemoryOwner of float, which you can dispose or return to the pool.</returns>
    private IMemoryOwner<float> BuildVertexDataMemory(out int vertexCount, MeshType meshType)
    {
        var estimatedVertexCount = Chunk.ChunkVolume * 6 * 6 * 8;
        var memoryOwner = MemoryPool<float>.Shared.Rent(estimatedVertexCount);
        var span = memoryOwner.Memory.Span;
        var index = 0;
        var chunkVoxelSpan = Chunk.GetVoxelSpan();

        for (var x = 0; x < Chunk.ChunkSize; x++)
        for (var z = 0; z < Chunk.ChunkSize; z++)
        for (var y = 0; y < Chunk.ChunkSize; y++)
        {
            var voxelIndex = Chunk.GetVoxelIndex(new Position<int>(x, y, z));
            var voxel = chunkVoxelSpan[voxelIndex];
            var alpha = voxel.Rgba.A;
            
            // This is the only logic that changes
            switch (meshType)
            {
                case MeshType.Opaque:
                    // Skip non-opaque (A != 255) voxels
                    if (alpha != 255) continue;
                    break;
                case MeshType.Transparent:
                    // Skip opaque (A=255) and fully transparent/air (A=0) voxels
                    if (alpha is 255 or 0) continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meshType), meshType, null);
            }
            
            // The rest of the logic is shared
            AddVisibleFacesToSpan(span, chunkVoxelSpan, ref index, x, y, z, voxel);
        }

        vertexCount = index;
        return memoryOwner;
    }
    
    protected override IMemoryOwner<float> GetOpaqueVertexDataMemory(out int vertexCount)
    {
        return BuildVertexDataMemory(out vertexCount, MeshType.Opaque);
    }

    protected override IMemoryOwner<float> GetTransparentVertexDataMemory(out int vertexCount)
    {
        return BuildVertexDataMemory(out vertexCount, MeshType.Transparent);
    }
    
   

    /// <summary>
    ///     Sets up vertex attribute pointers for this mesh.
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
        var faceIndex = shaderProgram.GetAttribLocation("in_face_id_float");
        if (faceIndex == -1) return;
        GL.EnableVertexAttribArray(faceIndex);
        GL.VertexAttribPointer(faceIndex, 1, VertexAttribPointerType.Float, false, 8 * sizeof(float),
            (IntPtr)(7 * sizeof(float)));
    }

    /// <summary>
    ///     Computes the model matrix for this chunk based on its world position.
    /// </summary>
    /// <returns>A translation matrix placing this chunk in world space.</returns>
    public override Matrix4 GetModelMatrix()
    {
        // Calculate the translation for this chunk
        return Matrix4.CreateTranslation(
            Chunk.Position.X * Chunk.ChunkSize,
            Chunk.Position.Y * Chunk.ChunkSize,
            Chunk.Position.Z * Chunk.ChunkSize
        );
    }

    /// <summary>
    ///     Checks whether the voxel at the specified coordinates is "void" from the perspective of rendering
    ///     (i.e., out of bounds or transparent).
    /// </summary>
    /// <param name="x">X coordinate within the chunk.</param>
    /// <param name="y">Y coordinate within the chunk.</param>
    /// <param name="z">Z coordinate within the chunk.</param>
    /// <param name="currentAlpha">Alpha value of the current voxel.</param>
    /// <param name="voxelSpan">Span of all voxels in this chunk.</param>
    private bool IsVoid(int x, int y, int z, int currentAlpha, Span<Voxel> voxelSpan)
    {
        byte adjacentAlpha;
        var neighborLocalPos = new Position<int>(x, y, z);

        if (!IsWithinBounds(x, y, z))
        {
            // --- NEIGHBOR CHUNK LOGIC ---
            // 1. Get the global position of this neighbor
            var neighborGlobalPos = Chunk.LocalToGlobalPosition(neighborLocalPos);            
            // 2. Ask the world for the voxel at that global position
            var neighborVoxel = _voxelWorld.GetVoxel(neighborGlobalPos);
            adjacentAlpha = neighborVoxel.Rgba.A;
        }
        else
        {
            // --- SAME CHUNK LOGIC ---
            var idx = Chunk.GetVoxelIndex(neighborLocalPos);
            adjacentAlpha = voxelSpan[idx].Rgba.A;
        }

        // --- SHARED CULLING LOGIC ---
        
        // If adjacent is Air, always draw
        if (adjacentAlpha == 0) return true;
        
        bool isCurrentOpaque = (currentAlpha == 255);
        bool isAdjacentOpaque = (adjacentAlpha == 255);

        // Draw if one is opaque and the other is not
        // This correctly handles:
        //   Solid-Solid (false), Water-Water (false)
        //   Solid-Water (true), Water-Air (true)
        return isCurrentOpaque != isAdjacentOpaque;
    }

    private bool IsWithinBounds(int x, int y, int z)
    {
        return x >= 0 && x < Chunk.ChunkSize &&
               y >= 0 && y < Chunk.ChunkSize &&
               z >= 0 && z < Chunk.ChunkSize;
    }

    /// <summary>
    ///     For a given voxel, checks each face to determine if it should be rendered.
    ///     If visible, adds the corresponding vertices to the shared vertex span.
    /// </summary>
    /// <param name="span">The vertex buffer span.</param>
    /// <param name="voxelSpan">Span of this chunk's voxels.</param>
    /// <param name="index">Reference to the current index in the vertex buffer span.</param>
    /// <param name="x">Voxel X coordinate.</param>
    /// <param name="y">Voxel Y coordinate.</param>
    /// <param name="z">Voxel Z coordinate.</param>
    /// <param name="voxel">The voxel being processed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddVisibleFacesToSpan(Span<float> span, Span<Voxel> voxelSpan, ref int index,
        int x, int y, int z, Voxel voxel)
    {
        int alpha = voxel.Rgba.A;

        // Top face
        if (IsVoid(x, y + 1, z, alpha, voxelSpan))
            AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Top));

        // Bottom face
        if (IsVoid(x, y - 1, z, alpha, voxelSpan))
            AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Bottom));

        // Right face
        if (IsVoid(x + 1, y, z, alpha, voxelSpan))
            AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Right));

        // Left face
        if (IsVoid(x - 1, y, z, alpha, voxelSpan))
            AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Left));

        // Back face
        if (IsVoid(x, y, z - 1, alpha, voxelSpan))
            AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Back));

        // Front face
        if (IsVoid(x, y, z + 1, alpha, voxelSpan))
            AddVerticesToSpan(span, ref index, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Front));
    }

    /// <summary>
    ///     Adds each vertex to the shared vertex buffer span.
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