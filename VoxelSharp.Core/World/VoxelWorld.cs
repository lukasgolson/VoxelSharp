using Microsoft.Extensions.Logging;
using VoxelSharp.Core.ECS.Jobs;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Resources;

namespace VoxelSharp.Core.World;

public class VoxelWorld
{
    public readonly Dictionary<Position<int>, Chunk> ChunkArray = new();


    public int ChunkSize => 16;
    
    private readonly Arch.Core.World _ecsWorld;
    private readonly HashSet<Position<int>> _pendingChunkRequests = [];


    private readonly ILogger<VoxelWorld> _logger;

    public VoxelWorld(ILogger<VoxelWorld> logger, Arch.Core.World ecsWorld) 
    {
        _logger = logger;
        _ecsWorld = ecsWorld;
    }
    
    /// <summary>
    /// Asynchronously requests a chunk to be loaded.
    /// If the chunk is not loaded and not already pending,
    /// this will create a new generation job for it.
    /// </summary>
    public void RequestChunk(Position<int> chunkPos)
    {
        if (IsChunkLoaded(chunkPos))
            return;

        if (_pendingChunkRequests.Contains(chunkPos))
            return;

        _pendingChunkRequests.Add(chunkPos);
        _ecsWorld.Create(
            new ChunkPosition { X = chunkPos.X, Y = chunkPos.Y, Z = chunkPos.Z },
            new NeedsGeneration(),
            new ChunkData { Chunk = null }
        );
    }

    public bool IsChunkLoaded(Position<int> chunkPos)
    {
        return ChunkArray.ContainsKey(chunkPos);
    }
    
    public void CommitChunk(Chunk chunk)
    {
        if (ChunkArray.TryAdd(chunk.Position, chunk))
        {
            _pendingChunkRequests.Remove(chunk.Position);
        }
    }

    public Chunk? GetChunk(Position<int> chunkPos)
    {
        if (ChunkArray.TryGetValue(chunkPos, out var chunk))
        {
            return chunk;
        }
        return null; // Just return null if not found
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

   
}