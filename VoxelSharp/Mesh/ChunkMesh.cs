using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Structs;
using VoxelSharp.World;

namespace VoxelSharp.Mesh;

public class ChunkMesh(Chunk chunk) : BaseMesh
{
    public override void Render()
    {
        if (chunk.IsDirty || !IsInitialized())
        {
            SetupMesh(8);
            chunk.IsDirty = false;
        }

        base.Render();
    }

    public override List<float> GetVertexData()
    {
        var vertexData = new List<float>
        {
            Capacity = chunk.ChunkVolume * 18 * 5
        };

        for (int x = 0; x < chunk.ChunkSize; ++x)
        {
            for (int z = 0; z < chunk.ChunkSize; ++z)
            {
                for (int y = 0; y < chunk.ChunkSize; ++y)
                {
                    Voxel voxel = chunk.Voxels[chunk.GetVoxelIndex(new Position<int>(x, y, z))];

                    if (voxel.Color.A == 0) continue;


                    // Top face
                    if (IsVoid(x, y + 1, z, voxel.Color.A, chunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Top));
                    }

                    // Bottom face
                    if (IsVoid(x, y - 1, z, voxel.Color.A, chunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Bottom));
                    }

                    // Right face
                    if (IsVoid(x + 1, y, z, voxel.Color.A, chunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Right));
                    }

                    // Left face
                    if (IsVoid(x - 1, y, z, voxel.Color.A, chunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Left));
                    }

                    // Back face
                    if (IsVoid(x, y, z - 1, voxel.Color.A, chunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Back));
                    }

                    // Front face
                    if (IsVoid(x, y, z + 1, voxel.Color.A, chunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Front));
                    }
                }
            }
        }

        return vertexData;
    }

    protected override void SetVertexAttributes()
    {
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), IntPtr.Zero);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float),
            (IntPtr)(3 * sizeof(float)));

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 8 * sizeof(float),
            (IntPtr)(7 * sizeof(float)));
    }

    private bool IsVoid(int x, int y, int z, int currentAlpha, Voxel[] voxels)
    {
        if (x < 0 || x >= chunk.ChunkSize || y < 0 || y >= chunk.ChunkSize || z < 0 ||
            z >= chunk.ChunkSize)
        {
            return true;
        }

        return voxels[chunk.GetVoxelIndex(new Position<int>(x, y, z))].Color.A != currentAlpha;
    }

    public bool IsInitialized()
    {
        return Vao != 0;
    }

    public override Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateTranslation(new Vector3(chunk.Position.X * chunk.ChunkSize,
            chunk.Position.Y * chunk.ChunkSize,
            chunk.Position.Z * chunk.ChunkSize));
    }

    public static void AddVerticesToData(List<float> data, IEnumerable<VoxelVertex> vertices)
    {
        foreach (var vertex in vertices)
        {
            data.Add(vertex.X);
            data.Add(vertex.Y);
            data.Add(vertex.Z);
            data.Add(vertex.R);
            data.Add(vertex.G);
            data.Add(vertex.B);
            data.Add(vertex.A);
            data.Add(vertex.FaceId);
        }
    }
}