using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelSharp.Structs;
using VoxelSharp.World;

namespace VoxelSharp.Mesh;

public class ChunkMesh(Chunk chunk) : BaseMesh
{
    private Chunk _associatedChunk = chunk;


    public override void Render()
    {
        if (_associatedChunk.IsDirty || !IsInitialized())
        {
            SetupMesh(8);
            _associatedChunk.IsDirty = false;
        }

        base.Render();
    }

    public override List<float> GetVertexData()
    {
        var vertexData = new List<float>
        {
            Capacity = _associatedChunk.ChunkVolume * 18 * 5
        };

        for (int x = 0; x < _associatedChunk.ChunkSize; ++x)
        {
            for (int z = 0; z < _associatedChunk.ChunkSize; ++z)
            {
                for (int y = 0; y < _associatedChunk.ChunkSize; ++y)
                {
                    var voxel = _associatedChunk.Voxels[_associatedChunk.GetVoxelIndex(new Position<int>(x, y, z))];
                    if (voxel.Color.A == 0) continue;

                    // Top face
                    if (IsVoid(x, y + 1, z, voxel.Color.A, _associatedChunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Top));
                    }

                    // Bottom face
                    if (IsVoid(x, y - 1, z, voxel.Color.A, _associatedChunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Bottom));
                    }

                    // Right face
                    if (IsVoid(x + 1, y, z, voxel.Color.A, _associatedChunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Right));
                    }

                    // Left face
                    if (IsVoid(x - 1, y, z, voxel.Color.A, _associatedChunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Left));
                    }

                    // Back face
                    if (IsVoid(x, y, z - 1, voxel.Color.A, _associatedChunk.Voxels))
                    {
                        AddVerticesToData(vertexData, VoxelVertex.CreateFace(x, y, z, voxel, FaceId.Back));
                    }

                    // Front face
                    if (IsVoid(x, y, z + 1, voxel.Color.A, _associatedChunk.Voxels))
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

    public bool IsVoid(int x, int y, int z, int currentAlpha, Voxel[] voxels)
    {
        if (x < 0 || x >= _associatedChunk.ChunkSize || y < 0 || y >= _associatedChunk.ChunkSize || z < 0 ||
            z >= _associatedChunk.ChunkSize)
        {
            return true;
        }

        return voxels[_associatedChunk.GetVoxelIndex(new Position<int>(x, y, z))].Color.A != currentAlpha;
    }

    public void AssociateChunk(Chunk chunk)
    {
        this._associatedChunk = chunk;
    }

    public bool IsInitialized()
    {
        return this.Vao != 0;
    }

    public override Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateTranslation(new Vector3(_associatedChunk.Position.X * _associatedChunk.ChunkSize,
            _associatedChunk.Position.Y * _associatedChunk.ChunkSize,
            _associatedChunk.Position.Z * _associatedChunk.ChunkSize));
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