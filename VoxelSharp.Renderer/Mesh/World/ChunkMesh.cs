using System.Buffers;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Rendering;

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


    private enum MeshType
    {
        Opaque,
        Transparent
    }

    public void UploadMeshData(MeshData data)
    {
        // Upload Opaque
        if (data.OpaqueCount > 0)
            UploadSection(data.OpaqueVertices, data.OpaqueCount, isTransparent: false);
        else
            OpaqueVertexCount = 0;

        // Upload Transparent
        if (data.TransparentCount > 0)
            UploadSection(data.TransparentVertices, data.TransparentCount, isTransparent: true);
        else
            TransparentVertexCount = 0;

        Chunk.IsDirty = false;

        // IMPORTANT: Release memory back to the pool
        data.OpaqueVertices.Dispose();
        data.TransparentVertices.Dispose();
    }

    private void UploadSection(IMemoryOwner<float> memory, int vertexCount, bool isTransparent)
    {
        // Mimic BaseMesh setup but with raw data
        // Note: You might need to change BaseMesh fields (_opaqueVao, etc) to 'protected'

        ref int vao = ref isTransparent ? ref _opaqueVao : ref _opaqueVao; // Just a placeholder, use actual fields
        ref int vbo = ref isTransparent ? ref _transparentVbo : ref _opaqueVbo;

        // Correct logic for accessing BaseMesh protected fields (assuming you changed them to protected):
        if (isTransparent)
        {
            SetupBuffers(ref _transparentVao, ref _transparentVbo, memory, vertexCount);
            TransparentVertexCount = vertexCount / 8;
        }
        else
        {
            SetupBuffers(ref _opaqueVao, ref _opaqueVbo, memory, vertexCount);
            OpaqueVertexCount = vertexCount / 8;
        }
    }

    private void SetupBuffers(ref int vao, ref int vbo, IMemoryOwner<float> memory, int vertexCount)
    {
        if (vao == 0) vao = GL.GenVertexArray();
        if (vbo == 0) vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        var span = memory.Memory.Span.Slice(0, vertexCount);
        GL.BufferData(BufferTarget.ArrayBuffer, span.Length * sizeof(float), ref span[0], BufferUsageHint.StaticDraw);

        // Attributes
        GL.EnableVertexAttribArray(0); // Pos
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1); // Color
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(2); // Face
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 8 * sizeof(float), 7 * sizeof(float));

        GL.BindVertexArray(0);
    }
    
    public static unsafe void GenerateMesh(Chunk chunk, 
        Span<Voxel> left, Span<Voxel> right, Span<Voxel> down, Span<Voxel> up, Span<Voxel> back, Span<Voxel> front,
        out MeshData meshData)
    {
        var chunkSize = chunk.ChunkSize;
        var chunkVol = chunk.ChunkVolume;
        var chunkVoxelSpan = chunk.GetVoxelSpan();
        
        // Allocate two buffers
        int estimatedSize = chunkVol * 6 * 6 * 8;
        var opaqueMem = MemoryPool<float>.Shared.Rent(estimatedSize);
        var transMem = MemoryPool<float>.Shared.Rent(estimatedSize / 2); // Usually smaller

        var opaqueSpan = opaqueMem.Memory.Span;
        var transSpan = transMem.Memory.Span;
        
        int opaqueIdx = 0;
        int transIdx = 0;
        int voxelLinearIdx = 0;

        // SINGLE PASS LOOP
        for (var y = 0; y < chunkSize; y++)
        {
            for (var z = 0; z < chunkSize; z++)
            {
                for (var x = 0; x < chunkSize; x++)
                {
                    var voxel = chunkVoxelSpan[voxelLinearIdx];
                    var alpha = voxel.Rgba.A;

                    if (alpha == 255)
                    {
                        // Write to Opaque Buffer
                        AddVisibleFacesToSpan(opaqueSpan, chunkVoxelSpan, ref opaqueIdx, 
                            x, y, z, voxelLinearIdx, voxel, chunkSize,
                            left, right, down, up, back, front);
                    }
                    else if (alpha != 0)
                    {
                        // Write to Transparent Buffer
                        AddVisibleFacesToSpan(transSpan, chunkVoxelSpan, ref transIdx, 
                            x, y, z, voxelLinearIdx, voxel, chunkSize,
                            left, right, down, up, back, front);
                    }

                    voxelLinearIdx++;
                }
            }
        }

        meshData = new MeshData
        {
            Chunk = chunk,
            OpaqueVertices = opaqueMem,
            OpaqueCount = opaqueIdx,
            TransparentVertices = transMem,
            TransparentCount = transIdx
        };
    }

    private Position<int>? Position => Chunk?.Position;

    public override void RenderOpaque(Shader shaderProgram)
    {
       
        if (!IsOpaqueInitialized || OpaqueVertexCount == 0) return;

        shaderProgram.SetUniform("m_model", GetModelMatrix());
        base.RenderOpaque(shaderProgram);
    }

    public override void RenderTransparent(Shader shaderProgram)
    {
        if (!IsTransparentInitialized || TransparentVertexCount == 0) return;

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
        var chunk = Chunk;
        var chunkSize = chunk.ChunkSize;
        var chunkPos = chunk.Position;


        var cLeft = _voxelWorld.GetChunk(chunkPos - Position<int>.Right);
        var leftSpan = cLeft != null ? cLeft.GetVoxelSpan() : Span<Voxel>.Empty;

        var cRight = _voxelWorld.GetChunk(chunkPos + Position<int>.Right);
        var rightSpan = cRight != null ? cRight.GetVoxelSpan() : Span<Voxel>.Empty;

        var cDown = _voxelWorld.GetChunk(chunkPos - Position<int>.Up);
        var downSpan = cDown != null ? cDown.GetVoxelSpan() : Span<Voxel>.Empty;

        var cUp = _voxelWorld.GetChunk(chunkPos + Position<int>.Up);
        var upSpan = cUp != null ? cUp.GetVoxelSpan() : Span<Voxel>.Empty;

        var cBack = _voxelWorld.GetChunk(chunkPos - Position<int>.Forward);
        var backSpan = cBack != null ? cBack.GetVoxelSpan() : Span<Voxel>.Empty;

        var cFront = _voxelWorld.GetChunk(chunkPos + Position<int>.Forward);
        var frontSpan = cFront != null ? cFront.GetVoxelSpan() : Span<Voxel>.Empty;


        var estimatedVertexCount = Chunk.ChunkVolume * 6 * 6 * 8;
        var memoryOwner = MemoryPool<float>.Shared.Rent(estimatedVertexCount);
        var span = memoryOwner.Memory.Span;
        var index = 0;
        var chunkVoxelSpan = chunk.GetVoxelSpan();
        int voxelIndex = 0; // Track linear index

        for (var y = 0; y < chunkSize; y++)
        {
            for (var z = 0; z < chunkSize; z++)
            {
                for (var x = 0; x < chunkSize; x++)
                {
                    var voxel = chunkVoxelSpan[voxelIndex];
                    var alpha = voxel.Rgba.A;

                    bool process = meshType == MeshType.Opaque ? alpha == 255 : (alpha != 255 && alpha != 0);

                    if (process)
                    {
                        AddVisibleFacesToSpan(span, chunkVoxelSpan, ref index, x, y, z, voxelIndex, voxel, chunkSize,
                            leftSpan, rightSpan, downSpan, upSpan, backSpan, frontSpan);
                    }

                    voxelIndex++; // Increment linear index
                }
            }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNeighborIndex(int x, int y, int z, int chunkSize)
    {
        if (chunkSize == 16)
        {
            return x + (z << 4) + (y << 8);
        }

        return x + (z * chunkSize) + (y * chunkSize * chunkSize);
    }

    /// <summary>
    ///     Checks whether the voxel at the specified coordinates is "void" from the perspective of rendering
    ///     (i.e., out of bounds or transparent).
    /// </summary>
    private bool IsVoid(int x, int y, int z, int currentAlpha, Span<Voxel> localSpan, int chunkSize,
        Span<Voxel> left, Span<Voxel> right,
        Span<Voxel> down, Span<Voxel> up,
        Span<Voxel> back, Span<Voxel> front)
    {
        if ((uint)x < (uint)chunkSize && (uint)y < (uint)chunkSize && (uint)z < (uint)chunkSize)
        {
            var adjacentAlpha = localSpan[GetNeighborIndex(x, y, z, chunkSize)].Rgba.A;

            // Standard transparency check
            if (adjacentAlpha == 0) return true;
            return (currentAlpha == 255) != (adjacentAlpha == 255);
        }


        byte neighborAlpha;
        if (x < 0)
        {
            if (left.IsEmpty) return true;
            neighborAlpha = left[GetNeighborIndex(chunkSize - 1, y, z, chunkSize)].Rgba.A;
        }
        else if (x >= chunkSize)
        {
            if (right.IsEmpty) return true;
            neighborAlpha = right[GetNeighborIndex(0, y, z, chunkSize)].Rgba.A;
        }
        else if (y < 0)
        {
            if (down.IsEmpty) return true;
            neighborAlpha = down[GetNeighborIndex(x, chunkSize - 1, z, chunkSize)].Rgba.A;
        }
        else if (y >= chunkSize)
        {
            if (up.IsEmpty) return true;
            neighborAlpha = up[GetNeighborIndex(x, 0, z, chunkSize)].Rgba.A;
        }
        else if (z < 0)
        {
            if (back.IsEmpty) return true;
            neighborAlpha = back[GetNeighborIndex(x, y, chunkSize - 1, chunkSize)].Rgba.A;
        }
        else // if (z >= chunkSize)
        {
            if (front.IsEmpty) return true;
            neighborAlpha = front[GetNeighborIndex(x, y, 0, chunkSize)].Rgba.A;
        }

        if (neighborAlpha == 0) return true;
        return (currentAlpha == 255) != (neighborAlpha == 255);
    }

    /// <summary>
    ///     For a given voxel, checks each face to determine if it should be rendered.
    ///     If visible, adds the corresponding vertices to the shared vertex span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddVisibleFacesToSpan(Span<float> span, Span<Voxel> localSpan, ref int index,
        int x, int y, int z, int currentIdx, Voxel voxel, int size,
        Span<Voxel> left, Span<Voxel> right,
        Span<Voxel> down, Span<Voxel> up,
        Span<Voxel> back, Span<Voxel> front)
    {
        int currentAlpha = voxel.Rgba.A;
        int area = size * size;
        byte adjacentAlpha;

        // Pre-calculate color floats once per voxel to avoid doing it 6 times per face
        float r = voxel.Rgba.R / 255f;
        float g = voxel.Rgba.G / 255f;
        float b = voxel.Rgba.B / 255f;
        float a = currentAlpha / 255f;

        // --- TOP FACE (Y + 1) ---
        if (y == size - 1)
        {
            if (up.IsEmpty) adjacentAlpha = 0;
            else adjacentAlpha = up[x + z * size].Rgba.A;
        }
        else
        {
            adjacentAlpha = localSpan[currentIdx + area].Rgba.A;
        }

        if (adjacentAlpha == 0 || (currentAlpha == 255) != (adjacentAlpha == 255))
        {
            // Write 6 vertices directly. No structs. No arrays.
            // Winding: v0, v3, v2, v0, v2, v1
            // v0=(x, y+1, z), v1=(x+1, y+1, z), v2=(x+1, y+1, z+1), v3=(x, y+1, z+1)

            WriteVertex(span, ref index, x, y + 1, z, r, g, b, a, 0f); // v0
            WriteVertex(span, ref index, x, y + 1, z + 1, r, g, b, a, 0f); // v3
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, r, g, b, a, 0f); // v2
            WriteVertex(span, ref index, x, y + 1, z, r, g, b, a, 0f); // v0
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, r, g, b, a, 0f); // v2
            WriteVertex(span, ref index, x + 1, y + 1, z, r, g, b, a, 0f); // v1
        }

        // --- BOTTOM FACE (Y - 1) ---
        if (y == 0)
        {
            if (down.IsEmpty) adjacentAlpha = 0;
            else adjacentAlpha = down[x + z * size + (size - 1) * area].Rgba.A;
        }
        else
        {
            adjacentAlpha = localSpan[currentIdx - area].Rgba.A;
        }

        if (adjacentAlpha == 0 || (currentAlpha == 255) != (adjacentAlpha == 255))
        {
            // Winding: v0, v1, v2, v0, v2, v3
            // v0=(x, y, z), v1=(x+1, y, z), v2=(x+1, y, z+1), v3=(x, y, z+1)
            WriteVertex(span, ref index, x, y, z, r, g, b, a, 1f); // v0
            WriteVertex(span, ref index, x + 1, y, z, r, g, b, a, 1f); // v1
            WriteVertex(span, ref index, x + 1, y, z + 1, r, g, b, a, 1f); // v2
            WriteVertex(span, ref index, x, y, z, r, g, b, a, 1f); // v0
            WriteVertex(span, ref index, x + 1, y, z + 1, r, g, b, a, 1f); // v2
            WriteVertex(span, ref index, x, y, z + 1, r, g, b, a, 1f); // v3
        }

        // --- RIGHT FACE (X + 1) ---
        if (x == size - 1)
        {
            if (right.IsEmpty) adjacentAlpha = 0;
            else adjacentAlpha = right[z * size + y * area].Rgba.A;
        }
        else
        {
            adjacentAlpha = localSpan[currentIdx + 1].Rgba.A;
        }

        if (adjacentAlpha == 0 || (currentAlpha == 255) != (adjacentAlpha == 255))
        {
            // Winding: v0, v1, v2, v0, v2, v3
            // v0=(x+1, y, z), v1=(x+1, y+1, z), v2=(x+1, y+1, z+1), v3=(x+1, y, z+1)
            WriteVertex(span, ref index, x + 1, y, z, r, g, b, a, 2f); // v0
            WriteVertex(span, ref index, x + 1, y + 1, z, r, g, b, a, 2f); // v1
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, r, g, b, a, 2f); // v2
            WriteVertex(span, ref index, x + 1, y, z, r, g, b, a, 2f); // v0
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, r, g, b, a, 2f); // v2
            WriteVertex(span, ref index, x + 1, y, z + 1, r, g, b, a, 2f); // v3
        }

        // --- LEFT FACE (X - 1) ---
        if (x == 0)
        {
            if (left.IsEmpty) adjacentAlpha = 0;
            else adjacentAlpha = left[(size - 1) + z * size + y * area].Rgba.A;
        }
        else
        {
            adjacentAlpha = localSpan[currentIdx - 1].Rgba.A;
        }

        if (adjacentAlpha == 0 || (currentAlpha == 255) != (adjacentAlpha == 255))
        {
            // Winding: v0, v3, v2, v0, v2, v1
            // v0=(x, y, z), v1=(x, y+1, z), v2=(x, y+1, z+1), v3=(x, y, z+1)
            WriteVertex(span, ref index, x, y, z, r, g, b, a, 3f); // v0
            WriteVertex(span, ref index, x, y, z + 1, r, g, b, a, 3f); // v3
            WriteVertex(span, ref index, x, y + 1, z + 1, r, g, b, a, 3f); // v2
            WriteVertex(span, ref index, x, y, z, r, g, b, a, 3f); // v0
            WriteVertex(span, ref index, x, y + 1, z + 1, r, g, b, a, 3f); // v2
            WriteVertex(span, ref index, x, y + 1, z, r, g, b, a, 3f); // v1
        }

        // --- FRONT FACE (Z + 1) ---
        if (z == size - 1)
        {
            if (front.IsEmpty) adjacentAlpha = 0;
            else adjacentAlpha = front[x + y * area].Rgba.A;
        }
        else
        {
            adjacentAlpha = localSpan[currentIdx + size].Rgba.A;
        }

        if (adjacentAlpha == 0 || (currentAlpha == 255) != (adjacentAlpha == 255))
        {
            // Winding: v0, v2, v1, v0, v3, v2
            // v0=(x, y, z+1), v1=(x, y+1, z+1), v2=(x+1, y+1, z+1), v3=(x+1, y, z+1)
            WriteVertex(span, ref index, x, y, z + 1, r, g, b, a, 5f); // v0
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, r, g, b, a, 5f); // v2
            WriteVertex(span, ref index, x, y + 1, z + 1, r, g, b, a, 5f); // v1
            WriteVertex(span, ref index, x, y, z + 1, r, g, b, a, 5f); // v0
            WriteVertex(span, ref index, x + 1, y, z + 1, r, g, b, a, 5f); // v3
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, r, g, b, a, 5f); // v2
        }

        // --- BACK FACE (Z - 1) ---
        if (z == 0)
        {
            if (back.IsEmpty) adjacentAlpha = 0;
            else adjacentAlpha = back[x + (size - 1) * size + y * area].Rgba.A;
        }
        else
        {
            adjacentAlpha = localSpan[currentIdx - size].Rgba.A;
        }

        if (adjacentAlpha == 0 || (currentAlpha == 255) != (adjacentAlpha == 255))
        {
            // Winding: v0, v1, v2, v0, v2, v3
            // v0=(x, y, z), v1=(x, y+1, z), v2=(x+1, y+1, z), v3=(x+1, y, z)
            WriteVertex(span, ref index, x, y, z, r, g, b, a, 4f); // v0
            WriteVertex(span, ref index, x, y + 1, z, r, g, b, a, 4f); // v1
            WriteVertex(span, ref index, x + 1, y + 1, z, r, g, b, a, 4f); // v2
            WriteVertex(span, ref index, x, y, z, r, g, b, a, 4f); // v0
            WriteVertex(span, ref index, x + 1, y + 1, z, r, g, b, a, 4f); // v2
            WriteVertex(span, ref index, x + 1, y, z, r, g, b, a, 4f); // v3
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteVertex(Span<float> span, ref int index, float x, float y, float z, float r, float g, float b,
        float a, float faceId)
    {
        span[index] = x;
        span[index + 1] = y;
        span[index + 2] = z;
        span[index + 3] = r;
        span[index + 4] = g;
        span[index + 5] = b;
        span[index + 6] = a;
        span[index + 7] = faceId;
        index += 8;
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