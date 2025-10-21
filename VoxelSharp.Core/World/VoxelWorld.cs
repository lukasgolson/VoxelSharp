using Microsoft.Extensions.Logging;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Resources;

namespace VoxelSharp.Core.World;

public class VoxelWorld
{
    public readonly Dictionary<Position<int>, Chunk> ChunkArray = new();


    public int ChunkSize => 16;

    private readonly IWorldGenerator _worldGenerator;

    private readonly ILogger<VoxelWorld> _logger;

    public VoxelWorld(IWorldGenerator worldGenerator, ILogger<VoxelWorld> logger)
    {
        _worldGenerator = worldGenerator;

        _logger = logger;
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

        _worldGenerator.GenerateChunkHeightmap(chunk);
        
        _worldGenerator.DecorateChunkHeightmap(chunk);

        ChunkArray.Add(chunk.Position, chunk);

        _logger.LogInformation("Loaded chunk at position {0}", chunk.Position);
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
        
        if (localCoords.X == 0)
            SetChunkDirty(chunkCoords - Position<int>.Right);
        else if (localCoords.X == ChunkSize - 1)
            SetChunkDirty(chunkCoords + Position<int>.Right);

        if (localCoords.Y == 0)
            SetChunkDirty(chunkCoords - Position<int>.Up);
        else if (localCoords.Y == ChunkSize - 1)
            SetChunkDirty(chunkCoords + Position<int>.Up);

        if (localCoords.Z == 0)
            SetChunkDirty(chunkCoords - Position<int>.Forward);
        else if (localCoords.Z == ChunkSize - 1)
            SetChunkDirty(chunkCoords + Position<int>.Forward);
    }
    
    private void SetChunkDirty(Position<int> chunkPos)
    {
        if (IsChunkLoaded(chunkPos))
        {
            ChunkArray[chunkPos].IsDirty = true;
        }
    }

    public Position<int> GetChunkCoordinates(Position<int> worldCoords)
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
    
    public Voxel GetVoxelReadOnly(Position<int> worldPos)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        // Check if the chunk is loaded
        if (!IsChunkLoaded(chunkCoords))
        {
            // Do NOT generate the chunk. Return Air.
            return new Voxel(Rgba.Transparent);
        }

        // Chunk is loaded, so we can safely get the voxel
        return ChunkArray[chunkCoords].GetVoxel(localCoords);
    }
    
}