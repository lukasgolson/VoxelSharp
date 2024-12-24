using VoxelSharp.World;

namespace VoxelSharp.Renderer.Mesh.World;

public struct VoxelVertex
{
    public readonly float X; // Position
    public readonly float Y; // Position
    public readonly float Z; // Position
    public readonly float R; // Color components
    public readonly float G; // Color components
    public readonly float B; // Color components
    public readonly float A; // Color components
    public readonly float FaceId; // Identifier for the face


    public VoxelVertex(int x, int y, int z, Voxel voxel, FaceId faceId)
    {
        X = x;
        Y = y;
        Z = z;
        R = voxel.Color.R / 255.0f;
        G = voxel.Color.G / 255.0f;
        B = voxel.Color.B / 255.0f;
        A = voxel.Color.A / 255.0f;
        FaceId = (float)faceId;
    }


    public static IEnumerable<VoxelVertex> CreateFace(int x, int y, int z, Voxel voxel, FaceId faceId)
    {
        return faceId switch
        {
            World.FaceId.Top =>
            [
                new VoxelVertex(x + 0, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 1, voxel, faceId)
            ],
            World.FaceId.Bottom =>
            [
                new VoxelVertex(x + 0, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 0, z + 1, voxel, faceId)
            ],
            World.FaceId.Right =>
            [
                new VoxelVertex(x + 1, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 1, voxel, faceId)
            ],
            World.FaceId.Left =>
            [
                new VoxelVertex(x + 0, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 0, z + 1, voxel, faceId)
            ],
            World.FaceId.Back =>
            [
                new VoxelVertex(x + 0, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 0, y + 0, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 0, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 0, voxel, faceId)
            ],
            World.FaceId.Front =>
            [
                new VoxelVertex(x + 0, y + 0, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 0, y + 0, z + 1, voxel, faceId),
                new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId),
                new VoxelVertex(x + 1, y + 0, z + 1, voxel, faceId)
            ],
            _ => throw new ArgumentException("Invalid face ID.")
        };
    }
}

public enum FaceId : byte
{
    Top = 0,
    Bottom = 1,
    Right = 2,
    Left = 3,
    Back = 4,
    Front = 5
}