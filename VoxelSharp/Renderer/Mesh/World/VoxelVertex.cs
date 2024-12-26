using VoxelSharp.World;

namespace VoxelSharp.Renderer.Mesh.World;

public readonly struct VoxelVertex
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;
    public readonly float R; // Color components (Red)
    public readonly float G; // Color components (Green)
    public readonly float B; // Color components (Blue)
    public readonly float A = 1.0f; // Alpha (fully opaque)
    public readonly float id; // Identifier for the face

    public VoxelVertex(int x, int y, int z, Voxel voxel, FaceId faceId, bool debug = false)
    {
        X = x;
        Y = y;
        Z = z;

        if (debug)
        {
            // Assign unique colors based on FaceId for debugging
            (R, G, B) = faceId switch
            {
                FaceId.Top => (1.0f, 0.0f, 0.0f), // Red
                FaceId.Bottom => (0.0f, 1.0f, 0.0f), // Green
                FaceId.Right => (0.0f, 0.0f, 1.0f), // Blue
                FaceId.Left => (1.0f, 1.0f, 0.0f), // Yellow
                FaceId.Back => (0.0f, 1.0f, 1.0f), // Cyan
                FaceId.Front => (1.0f, 0.0f, 1.0f), // Magenta
                _ => (1.0f, 1.0f, 1.0f) // White (fallback)
            };
        }
        else
        {
            // Assign the color of the voxel to the vertex
            (R, G, B) = (voxel.Color.R, voxel.Color.G, voxel.Color.B);
        }
    }


    public static IEnumerable<VoxelVertex> CreateFace(int x, int y, int z, Voxel voxel, FaceId faceId)
    {
        // top face

        switch (faceId)
        {
            case FaceId.Top:
            {
                var v0 = new VoxelVertex(x, y + 1, z, voxel, faceId);
                var v1 = new VoxelVertex(x + 1, y + 1, z, voxel, faceId);
                var v2 = new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId);
                var v3 = new VoxelVertex(x, y + 1, z + 1, voxel, faceId);

                // add in order: 0,3,2,0,2,1
                return [v0, v3, v2, v0, v2, v1];
            }
            case FaceId.Bottom:
            {
                var v0 = new VoxelVertex(x, y, z, voxel, faceId);
                var v1 = new VoxelVertex(x + 1, y, z, voxel, faceId);
                var v2 = new VoxelVertex(x + 1, y, z + 1, voxel, faceId);
                var v3 = new VoxelVertex(x, y, z + 1, voxel, faceId);

                // add in order: 0,2,3,0,1,2
                return [v0, v2, v3, v0, v1, v2];
            }
            case FaceId.Right:
            {
                var v0 = new VoxelVertex(x + 1, y, z, voxel, faceId);
                var v1 = new VoxelVertex(x + 1, y + 1, z, voxel, faceId);
                var v2 = new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId);
                var v3 = new VoxelVertex(x + 1, y, z + 1, voxel, faceId);

                // add in order: 0, 1, 2, 0, 2, 3
                return [v0, v1, v2, v0, v2, v3];
            }
            case FaceId.Left:
            {
                var v0 = new VoxelVertex(x, y, z, voxel, faceId);
                var v1 = new VoxelVertex(x, y + 1, z, voxel, faceId);
                var v2 = new VoxelVertex(x, y + 1, z + 1, voxel, faceId);
                var v3 = new VoxelVertex(x, y, z + 1, voxel, faceId);

                // add in order: 0, 2, 1, 0, 3, 2
                return [v0, v2, v1, v0, v3, v2];
            }
            case FaceId.Back:
            {
                var v0 = new VoxelVertex(x, y, z, voxel, faceId);
                var v1 = new VoxelVertex(x, y + 1, z, voxel, faceId);
                var v2 = new VoxelVertex(x + 1, y + 1, z, voxel, faceId);
                var v3 = new VoxelVertex(x + 1, y, z, voxel, faceId);

                // add in order: 0, 1, 2, 0, 2, 3
                return [v0, v1, v2, v0, v2, v3];
            }
            case FaceId.Front:
            {
                var v0 = new VoxelVertex(x, y, z + 1, voxel, faceId);
                var v1 = new VoxelVertex(x, y + 1, z + 1, voxel, faceId);
                var v2 = new VoxelVertex(x + 1, y + 1, z + 1, voxel, faceId);
                var v3 = new VoxelVertex(x + 1, y, z + 1, voxel, faceId);

                // add in order: 0, 2, 1, 0, 3, 2
                return [v0, v2, v1, v0, v3, v2];
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(faceId), faceId, "Invalid face id");
        }
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