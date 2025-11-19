using System.Buffers;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Helpers;
using VoxelSharp.Renderer.Rendering;

namespace VoxelSharp.Renderer.Mesh.World;

public class ChunkMesh : BaseMesh<int>
{
    public Chunk? Chunk { get; }

    public ChunkMesh(Chunk chunk)
    {
        Chunk = chunk;
    }


  

    public void UploadMeshData(MeshData data)
    {
        if (data.OpaqueCount > 0)
            UploadSection(data.OpaqueVertices, data.OpaqueCount, isTransparent: false);
        else
            OpaqueVertexCount = 0;

        if (data.TransparentCount > 0)
            UploadSection(data.TransparentVertices, data.TransparentCount, isTransparent: true);
        else
            TransparentVertexCount = 0;

        Chunk.IsDirty = false;

        // RETURN to custom pool
        MeshBufferPool.Return(data.OpaqueVertices);
        MeshBufferPool.Return(data.TransparentVertices);
    }

    private void UploadSection(int[] vertexArray, int vertexCount, bool isTransparent)
    {
        if (isTransparent)
        {
            SetupBuffers(ref _transparentVao, ref _transparentVbo, vertexArray, vertexCount);
            TransparentVertexCount = vertexCount / 2;
        }
        else
        {
            SetupBuffers(ref _opaqueVao, ref _opaqueVbo, vertexArray, vertexCount);
            OpaqueVertexCount = vertexCount / 2;
        }
    }

    private void SetupBuffers(ref int vao, ref int vbo, int[] vertexArray, int vertexCount)
    {
        if (vao == 0) vao = GL.GenVertexArray();
        if (vbo == 0) vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        // Use AsSpan with length to slice the array correctly
        var span = vertexArray.AsSpan(0, vertexCount);

        GL.BufferData(BufferTarget.ArrayBuffer, span.Length * sizeof(int), ref span[0], BufferUsageHint.StaticDraw);

        SetVertexAttributes(null!);

        GL.BindVertexArray(0);
    }

    protected override void SetVertexAttributes(Shader shaderProgram)
    {
        // Stride is 8 bytes (2 ints)
        int stride = 2 * sizeof(int);

        // Attribute 0: Data (Position packed) - integer input
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribIPointer(0, 1, VertexAttribIntegerType.UnsignedInt, stride, IntPtr.Zero);

        // Attribute 1: Color (RGBA packed) - normalized float input
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, stride, (IntPtr)sizeof(int));
    }
    
    public static void GenerateMesh(Chunk chunk, 
        Span<Voxel> left, Span<Voxel> right, Span<Voxel> down, Span<Voxel> up, Span<Voxel> back, Span<Voxel> front,
        out MeshData meshData)
    {
        var chunkSize = chunk.ChunkSize;
        var chunkVol = chunk.ChunkVolume;
        var chunkVoxelSpan = chunk.GetVoxelSpan();
        
        // 128 ints per voxel is enough space
        int estimatedSize = chunkVol * 128;

        // RENT from custom pool
        int[] opaqueArray = MeshBufferPool.Rent(estimatedSize);
        int[] transArray = MeshBufferPool.Rent(estimatedSize);

        var opaqueSpan = opaqueArray.AsSpan();
        var transSpan = transArray.AsSpan();
        
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
                        AddVisibleFacesToSpan(opaqueSpan, chunkVoxelSpan, ref opaqueIdx, 
                            x, y, z, voxelLinearIdx, voxel, chunkSize,
                            left, right, down, up, back, front);
                    }
                    else if (alpha != 0)
                    {
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
            OpaqueVertices = opaqueArray, // Store array
            OpaqueCount = opaqueIdx,
            TransparentVertices = transArray, // Store array
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteVertex(Span<int> span, ref int index, int x, int y, int z, uint color, int faceId)
    {
        // Pack Position + FaceId into one integer
        // X: 5 bits, Y: 5 bits, Z: 5 bits, Face: 3 bits
        // Layout: [Face:3][Z:5][Y:5][X:5]

        int data = (x & 0x1F) |
                   ((y & 0x1F) << 5) |
                   ((z & 0x1F) << 10) |
                   ((faceId & 0x7) << 15);

        span[index] = data;
        span[index + 1] = (int)color; // Cast uint color to int
        index += 2;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddVisibleFacesToSpan(Span<int> span, Span<Voxel> localSpan, ref int index,
        int x, int y, int z, int currentIdx, Voxel voxel, int size,
        Span<Voxel> left, Span<Voxel> right,
        Span<Voxel> down, Span<Voxel> up,
        Span<Voxel> back, Span<Voxel> front)
    {
        int currentAlpha = voxel.Rgba.A;
        int area = size * size;
        byte adjacentAlpha;

        // Pack color once
        uint color = (uint)(voxel.Rgba.R | (voxel.Rgba.G << 8) | (voxel.Rgba.B << 16) | (voxel.Rgba.A << 24));

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
            // Face 0: Top
            WriteVertex(span, ref index, x,     y + 1, z,     color, 0);
            WriteVertex(span, ref index, x,     y + 1, z + 1, color, 0);
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, color, 0);
            WriteVertex(span, ref index, x,     y + 1, z,     color, 0);
            WriteVertex(span, ref index, x + 1, y + 1, z + 1, color, 0);
            WriteVertex(span, ref index, x + 1, y + 1, z,     color, 0);
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
            // Face 1: Bottom
            WriteVertex(span, ref index, x,     y, z,     color, 1);
            WriteVertex(span, ref index, x + 1, y, z,     color, 1);
            WriteVertex(span, ref index, x + 1, y, z + 1, color, 1);
            WriteVertex(span, ref index, x,     y, z,     color, 1);
            WriteVertex(span, ref index, x + 1, y, z + 1, color, 1);
            WriteVertex(span, ref index, x,     y, z + 1, color, 1);
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
             // Face 2: Right
             WriteVertex(span, ref index, x + 1, y,     z,     color, 2);
             WriteVertex(span, ref index, x + 1, y + 1, z,     color, 2);
             WriteVertex(span, ref index, x + 1, y + 1, z + 1, color, 2);
             WriteVertex(span, ref index, x + 1, y,     z,     color, 2);
             WriteVertex(span, ref index, x + 1, y + 1, z + 1, color, 2);
             WriteVertex(span, ref index, x + 1, y,     z + 1, color, 2);
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
             // Face 3: Left
             WriteVertex(span, ref index, x, y,     z,     color, 3);
             WriteVertex(span, ref index, x, y,     z + 1, color, 3);
             WriteVertex(span, ref index, x, y + 1, z + 1, color, 3);
             WriteVertex(span, ref index, x, y,     z,     color, 3);
             WriteVertex(span, ref index, x, y + 1, z + 1, color, 3);
             WriteVertex(span, ref index, x, y + 1, z,     color, 3);
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
             // Face 5: Front
             WriteVertex(span, ref index, x,     y,     z + 1, color, 5);
             WriteVertex(span, ref index, x + 1, y + 1, z + 1, color, 5);
             WriteVertex(span, ref index, x,     y + 1, z + 1, color, 5);
             WriteVertex(span, ref index, x,     y,     z + 1, color, 5);
             WriteVertex(span, ref index, x + 1, y,     z + 1, color, 5);
             WriteVertex(span, ref index, x + 1, y + 1, z + 1, color, 5);
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
             // Face 4: Back
             WriteVertex(span, ref index, x,     y,     z, color, 4);
             WriteVertex(span, ref index, x,     y + 1, z, color, 4);
             WriteVertex(span, ref index, x + 1, y + 1, z, color, 4);
             WriteVertex(span, ref index, x,     y,     z, color, 4);
             WriteVertex(span, ref index, x + 1, y + 1, z, color, 4);
             WriteVertex(span, ref index, x + 1, y,     z, color, 4);
        }
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
}