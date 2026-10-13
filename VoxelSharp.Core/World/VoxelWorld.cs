using Arch.Core;
using Microsoft.Extensions.Logging;
using VoxelSharp.Core.ECS.Jobs;
using VoxelSharp.Core.Structs;
using VoxelSharp.Resources;

namespace VoxelSharp.Core.World;

public class VoxelWorld
{
    public readonly Dictionary<Position<int>, Chunk> ChunkArray = new();


    public int ChunkSize => 16;

    /// <summary>
    /// The chunk position that chunk loading is currently centred on.
    /// Pending chunks closest to this position are generated first.
    /// </summary>
    public Position<int> LoadCenter { get; private set; }

    private readonly Arch.Core.World _ecsWorld;

    // Every chunk that is either loaded or waiting to be generated, mapped to its ECS entity
    private readonly Dictionary<Position<int>, Entity> _chunkEntities = new();


    private readonly ILogger<VoxelWorld> _logger;

    public VoxelWorld(ILogger<VoxelWorld> logger, Arch.Core.World ecsWorld) 
    {
        _logger = logger;
        _ecsWorld = ecsWorld;
    }

    /// <summary>
    /// Keeps the cube of chunks within <paramref name="loadRadius"/> of <paramref name="center"/> loaded,
    /// requesting any that are missing and unloading those that have moved out of range.
    /// </summary>
    /// <param name="center">The chunk position to load around.</param>
    /// <param name="loadRadius">How many chunks to load in each direction.</param>
    public void UpdateLoadedRegion(Position<int> center, int loadRadius)
    {
        LoadCenter = center;

        // Unload one chunk further out than we load, so moving back and forth
        // across a chunk border doesn't repeatedly unload and regenerate chunks
        var unloadRadius = loadRadius + 1;

        var chunksToUnload = _chunkEntities.Keys
            .Where(chunkPos => ChunkDistance(chunkPos, center) > unloadRadius)
            .ToList();

        foreach (var chunkPos in chunksToUnload)
            UnloadChunk(chunkPos);

        for (var x = -loadRadius; x <= loadRadius; x++)
        for (var y = -loadRadius; y <= loadRadius; y++)
        for (var z = -loadRadius; z <= loadRadius; z++)
            RequestChunk(center + new Position<int>(x, y, z));
    }

    /// <summary>
    /// The number of chunks between two chunk positions along the furthest axis.
    /// </summary>
    public static int ChunkDistance(Position<int> a, Position<int> b)
    {
        return System.Math.Max(System.Math.Abs(a.X - b.X),
            System.Math.Max(System.Math.Abs(a.Y - b.Y), System.Math.Abs(a.Z - b.Z)));
    }

    /// <summary>
    /// Asynchronously requests a chunk to be loaded.
    /// If the chunk is not loaded and not already pending,
    /// this will create a new generation job for it.
    /// </summary>
    public void RequestChunk(Position<int> chunkPos)
    {
        if (_chunkEntities.ContainsKey(chunkPos))
            return;

        var entity = _ecsWorld.Create(
            new ChunkPosition { X = chunkPos.X, Y = chunkPos.Y, Z = chunkPos.Z },
            new NeedsGeneration(),
            new ChunkData { Chunk = null }
        );

        _chunkEntities.Add(chunkPos, entity);
    }

    /// <summary>
    /// Unloads a chunk, or cancels its pending generation if it hasn't been generated yet.
    /// </summary>
    public void UnloadChunk(Position<int> chunkPos)
    {
        if (!_chunkEntities.Remove(chunkPos, out var entity))
            return;

        if (_ecsWorld.IsAlive(entity))
            _ecsWorld.Destroy(entity);

        // Neighbors need remeshing so faces that were hidden by this chunk are drawn again
        if (ChunkArray.Remove(chunkPos))
            SetNeighborsDirty(chunkPos);
    }

    public bool IsChunkLoaded(Position<int> chunkPos)
    {
        return ChunkArray.ContainsKey(chunkPos);
    }
    
    /// <summary>
    /// Adds a generated chunk to the world. Chunks that were unloaded while they were
    /// being generated are discarded.
    /// </summary>
    public void CommitChunk(Chunk chunk)
    {
        if (!_chunkEntities.TryGetValue(chunk.Position, out var entity) || !_ecsWorld.IsAlive(entity))
        {
            _logger.LogDebug("Discarding generated chunk {ChunkPos} as it is no longer requested", chunk.Position);
            return;
        }

        if (!ChunkArray.TryAdd(chunk.Position, chunk))
            return;

        // An older generation job for this position may finish before the current one, so clear both states
        if (_ecsWorld.Has<NeedsGeneration>(entity))
            _ecsWorld.Remove<NeedsGeneration>(entity);
        if (_ecsWorld.Has<Generating>(entity))
            _ecsWorld.Remove<Generating>(entity);

        _ecsWorld.Set(entity, new ChunkData { Chunk = chunk });
        _ecsWorld.Add<NeedsMeshing>(entity);

        // Neighbors were meshed as if this chunk was empty, so their border faces need culling
        SetNeighborsDirty(chunk.Position);
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

    private void SetNeighborsDirty(Position<int> chunkPos)
    {
        SetChunkDirty(chunkPos + Position<int>.Right);
        SetChunkDirty(chunkPos - Position<int>.Right);
        SetChunkDirty(chunkPos + Position<int>.Up);
        SetChunkDirty(chunkPos - Position<int>.Up);
        SetChunkDirty(chunkPos + Position<int>.Forward);
        SetChunkDirty(chunkPos - Position<int>.Forward);
    }

    public Position<int> GetChunkCoordinates(Position<int> worldCoords)
    {
        // Floor division, so negative coordinates map to negative chunks (e.g. -1 is in chunk -1, not 0)
        var x = Math.FloorDiv(worldCoords.X, ChunkSize);
        var y = Math.FloorDiv(worldCoords.Y, ChunkSize);
        var z = Math.FloorDiv(worldCoords.Z, ChunkSize);

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