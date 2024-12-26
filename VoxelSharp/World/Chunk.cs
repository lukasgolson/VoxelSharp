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
    
    
    // IDEA: Change the voxel array to a Memory<Voxel> to minimize heap allocations; Add a method to get a Memory<Voxel> slice for a specific chunk area
    // and update the ChunkMesh to use this method to get the voxel data for the mesh generation.
    // Further, allow methods to directly modify the Memory<Voxel> slice to update the voxel data.


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
#if DEBUG
            // fill the chunk with random colors
            Voxels[i] = new Voxel(Color.GetRandomColor());

#else
            // fill the chunk with transparent voxels
            Voxels[i] = new Voxel(Color.Transparent);

#endif
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