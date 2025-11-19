using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.World;

public class ChunkCommitSystem : IUpdatable
{
    private readonly VoxelWorld _voxelWorld;
    private readonly ILogger _logger;
    private readonly GeneratedChunkQueue _mailbox;
    private readonly ICameraParameters _camera; 

    private const int UnloadDistance = 11; 
    private float _unloadTimer; 
    private const float UnloadCheckInterval = 2f; 

    public ChunkCommitSystem(IGameLoop gameLoop, VoxelWorld voxelWorld, 
        ILogger<ChunkCommitSystem> logger, GeneratedChunkQueue mailbox,
        ICameraParameters camera) 
    {
        _voxelWorld = voxelWorld;
        _logger = logger;
        _mailbox = mailbox;
        _camera = camera;
        
        gameLoop.RegisterUpdateAction(this);
    }
    
    public void Update(double deltaTime)
    {
        // 1. LOADING: Commit new chunks from the Generation Queue
        while (_mailbox.ChunkQueue.TryDequeue(out var chunk))
        {
            _voxelWorld.CommitChunk(chunk);
        }

        // 2. UNLOADING: Remove old chunks that are too far away
        ProcessUnloading(deltaTime);
    }

    private void ProcessUnloading(double deltaTime)
    {
        // Rate limit this check so we don't iterate the dictionary 60 times a second
        _unloadTimer += (float)deltaTime;
        if (_unloadTimer < UnloadCheckInterval) return;
        _unloadTimer = 0;

        var camPos = _camera.Position;
        var playerChunkX = (int)(camPos.X / _voxelWorld.ChunkSize);
        var playerChunkZ = (int)(camPos.Z / _voxelWorld.ChunkSize);

        List<Position<int>> chunksToRemove = new();

        // Iterate over all currently loaded chunks
        foreach (var chunkPos in _voxelWorld.ChunkArray.Keys)
        {
            // Calculate Manhattan distance (simple grid distance)
            var dx = System.Math.Abs(chunkPos.X - playerChunkX);
            var dz = System.Math.Abs(chunkPos.Z - playerChunkZ);
            
            // If the chunk is outside the safe radius
            if (dx > UnloadDistance || dz > UnloadDistance)
            {
                chunksToRemove.Add(chunkPos);
            }
        }

        // Remove them safely
        foreach (var pos in chunksToRemove)
        {
            // Ensure you added the 'RemoveChunk' method to VoxelWorld from the previous step!
            _voxelWorld.RemoveChunk(pos);
        }

        if (chunksToRemove.Count > 0)
        {
            _logger.LogDebug("Unloaded {Count} chunks", chunksToRemove.Count);
        }
    }
}