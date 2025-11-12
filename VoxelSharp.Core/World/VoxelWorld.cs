using Microsoft.Extensions.Logging;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Resources;

namespace VoxelSharp.Core.World;

public class VoxelWorld
{
    public readonly Dictionary<Position<int>, Chunk> ChunkArray = new();


    public int ChunkSize => 16;
    
    private HashSet<Position<int>> PendingChunks = new();


    private readonly ILogger<VoxelWorld> _logger;

    public VoxelWorld(ILogger<VoxelWorld> logger)
    {
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


        //ChunkArray.Add(chunk.Position, chunk);

        //_logger.LogInformation("Loaded chunk at position {0}", chunk.Position);
    }

    public Chunk? GetChunk(Position<int> chunkPos)
    {
        if (!IsChunkLoaded(chunkPos))
        {
            return null; // Chunk doesn't exist in memory, just return null.
        }

        return ChunkArray[chunkPos]; // It exists, return it.
    }


    public Voxel GetVoxel(Position<int> worldPos)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        if (!IsChunkLoaded(chunkCoords))
        {
            // If not loaded, just return air
            _logger.LogDebug("Tried to load non-loaded chunk {ChunkCoords}", chunkCoords);
            return new Voxel(Rgba.Transparent);
        }

        // Chunk is loaded, so we can safely get the voxel
        return ChunkArray[chunkCoords].GetVoxel(localCoords);
    }


    public void SetVoxel(Position<int> worldPos, Voxel voxel)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        if (!IsChunkLoaded(chunkCoords))
        {
            _logger.LogDebug("Tried to set voxel in non-loaded chunk {ChunkCoords}", chunkCoords);

            return;
        }

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

    public bool IsChunkRequestPending(Position<int> chunkPos)
    {
        return PendingChunks.Contains(chunkPos);
    }

    public void SetChunkRequestPending(Position<int> chunkPos, bool pending)
    {
        if (pending)
        {
            PendingChunks.Add(chunkPos);
        }
        else
        {
            PendingChunks.Remove(chunkPos);
        }
    }
}