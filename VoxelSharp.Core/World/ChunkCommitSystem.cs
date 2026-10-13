using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Loop;

namespace VoxelSharp.Core.World;

public class ChunkCommitSystem : IUpdatable
{
    private readonly VoxelWorld _voxelWorld;
    private readonly ILogger _logger;
    private readonly GeneratedChunkQueue _mailbox;
    
    
    public ChunkCommitSystem(IGameLoop gameLoop, VoxelWorld voxelWorld, 
        ILogger<ChunkCommitSystem> logger, GeneratedChunkQueue mailbox)
    {
        _voxelWorld = voxelWorld;
        _logger = logger;
        _mailbox = mailbox;
        
        // Register this system to the main game loop
        gameLoop.RegisterUpdateAction(this);
    }
    
    
    public void Update(double deltaTime)
    {
        while (_mailbox.ChunkQueue.TryDequeue(out var chunk))
        {
            _logger.LogDebug("Found chunk");
            _voxelWorld.CommitChunk(chunk);
        }
    }
}