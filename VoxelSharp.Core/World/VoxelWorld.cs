using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.World;

public class VoxelWorld
{
    public readonly Dictionary<Position<int>, Chunk> ChunkArray = new();

    public const int InitialChunkSize = 8; // temporary placeholder until we have a proper world generation system in place
    private const int ChunkSize = 16;

    public VoxelWorld()
    {
        const int initialWorldVolume = InitialChunkSize * InitialChunkSize * InitialChunkSize;


        for (var i = 0; i < initialWorldVolume; i++)
        {
            var position = Position<int>.FromIndex(i, InitialChunkSize);
            LoadChunk(position);
        }
    }
    
    public bool IsChunkLoaded(Position<int> chunkPos)
    {
        return ChunkArray.ContainsKey(chunkPos);
    }
    
    private void LoadChunk(Position<int> chunkPos)
    {
        if (IsChunkLoaded(chunkPos))
            return;
        
        var chunk = new Chunk(chunkPos, ChunkSize);
        ChunkArray.Add(chunk.Position, chunk);
    }
    
    public Chunk GetChunk(Position<int> chunkPos)
    {
        if (!IsChunkLoaded(chunkPos))
            LoadChunk(chunkPos);
        
        return ChunkArray[chunkPos];
    }


    public Voxel GetVoxel(Position<int> worldPos)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);
        
        if (!IsChunkLoaded(chunkCoords))
            LoadChunk(chunkCoords);
        
        return ChunkArray[chunkCoords].GetVoxel(localCoords);

    }
    
    

    public void SetVoxel(Position<int> worldPos, Voxel voxel)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);
        
        if (!IsChunkLoaded(chunkCoords))
            LoadChunk(chunkCoords);
        
        var chunk = ChunkArray[chunkCoords];
        chunk.SetVoxel(localCoords, voxel);
    }

    private Position<int> GetChunkCoordinates(Position<int> worldCoords)
    {
        var x = worldCoords.X / ChunkSize;
        var y = worldCoords.Y / ChunkSize;
        var z = worldCoords.Z / ChunkSize;

        return new Position<int>(x, y, z);
    }

    private Position<int> GetLocalCoordinates(Position<int> worldCoords)
    {
        var x = Math.EModulo(worldCoords.X, ChunkSize);
        var y = Math.EModulo(worldCoords.Y, ChunkSize);
        var z = Math.EModulo(worldCoords.Z, ChunkSize);

        return new Position<int>(x, y, z);
    }
}