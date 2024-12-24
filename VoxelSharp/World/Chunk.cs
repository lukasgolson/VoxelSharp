using VoxelSharp.Structs;

namespace VoxelSharp.World;

public class Chunk
{
    public int ChunkSize { get; }
    public int ChunkArea { get; }
    public int ChunkVolume { get; init; }

    public bool IsDirty { get; set; }

    public Voxel[] Voxels { get; }

    public Position<int> Position { get; }


    public Chunk(Position<int> position, int chunkSize = 16)
    {
        
        Position = position;

        ChunkSize = chunkSize;
        ChunkArea = ChunkSize * ChunkSize;
        ChunkVolume = ChunkSize * ChunkSize * ChunkSize;

        Voxels = new Voxel[ChunkVolume];

        
        // fill the Voxels array with empty voxels
        for (int i = 0; i < ChunkVolume; i++)
        {
            Voxels[i] = new Voxel(Color.White);
        }
    }

    public bool SetVoxel(Position<int> position, Voxel value)
    {
        Voxels[GetVoxelIndex(position)] = value;
        IsDirty = true;

        return true; // TODO: Implement proper return value. True if successful, false if not.
    }

    public Voxel GetVoxel(Position<int> position)
    {
        return Voxels[GetVoxelIndex(position)];
    }

    public int GetVoxelIndex(Position<int> position)
    {
        return position.X + ChunkSize * position.Z + ChunkArea * position.Y;
    }
}