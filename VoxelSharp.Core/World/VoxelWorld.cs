using Microsoft.Extensions.Logging;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;

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

        _worldGenerator.GenerateChunk(chunk);

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
}