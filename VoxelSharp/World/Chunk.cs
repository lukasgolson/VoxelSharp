using VoxelSharp.Structs;

namespace VoxelSharp.World;

public class Chunk
{
    private const int ChunkSize = 16;
    private const int ChunkArea = ChunkSize * ChunkSize;

    private bool IsDirty { get; set; }

    private Voxel[] Voxels { get; } = new Voxel[ChunkSize * ChunkSize * ChunkSize];

    public Position<int> Position { get; }


    Chunk(Position<int> position)
    {
        Position = position;
    }

    void SetVoxel(Position<int> position, Voxel value)
    {
        Voxels[GetVoxelIndex(position)] = value;
        IsDirty = true;
    }

    Voxel GetVoxel(Position<int> position)
    {
        return Voxels[GetVoxelIndex(position)];
    }

    private static int GetVoxelIndex(Position<int> position)
    {
        return position.X + ChunkSize * position.Z + ChunkArea * position.Y;
    }
}