using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using Chunk = VoxelSharp.Core.World.Chunk;

namespace VoxelSharp.Core.ECS.Jobs;

public struct ChunkPosition
{
    public int X, Y, Z;
}

public struct NeedsGeneration;

public struct NeedsMeshing;

public struct ChunkData
{
    public Chunk? Chunk;
};

public class WorldGenerationSystem : IUpdatable
{
    private readonly IWorldGenerator _worldGenerator;
    private readonly ILogger<WorldGenerationSystem> _logger;
    private readonly VoxelWorld _voxelWorld;


    private readonly GeneratedChunkQueue _mailbox;

    private Arch.Core.World _world;

    private const int MaxChunksPerFrame = 16;

    public WorldGenerationSystem(IGameLoop gameLoop, Arch.Core.World world, IWorldGenerator worldGenerator,
        ILogger<WorldGenerationSystem> logger, VoxelWorld voxelWorld, GeneratedChunkQueue mailbox)
    {
        _worldGenerator = worldGenerator;
        _logger = logger;
        _world = world;
        _voxelWorld = voxelWorld;
        _mailbox = mailbox;

        // gameLoop.RegisterBackgroundUpdateAction(this, "Test");
        gameLoop.RegisterUpdateAction(this);
    }


    public void Update(double deltaTime)
    {
        using var commandBuffer = new CommandBuffer();
        var query = new QueryDescription().WithAll<ChunkPosition, ChunkData, NeedsGeneration>();

        // 1. COLLECT: Identify the batch of chunks to generate this frame
        var batch = new List<(Entity Entity, Chunk Chunk)>(MaxChunksPerFrame);
        int count = 0;

        // This part runs on the Main Thread (fast)
        _world.Query(in query, (Entity entity, ref ChunkPosition pos, ref ChunkData data) =>
        {
            // Stop once we fill our batch
            if (count >= MaxChunksPerFrame) return;

            // Create the chunk instance immediately
            var chunk = new Chunk(new Position<int>(pos.X, pos.Y, pos.Z), _voxelWorld.ChunkSize)
            {
                Entity = entity
            };

            // Assign the chunk to the ECS component now
            data.Chunk = chunk;

            batch.Add((entity, chunk));
            count++;
        });

        // If no chunks need generation, exit early
        if (batch.Count == 0) return;

        // 2. PROCESS: Run heavy calculations in parallel
        // This blocks the main thread until done, but distributes work across all CPU cores.
        Parallel.ForEach(batch, item =>
        {
            var chunk = item.Chunk;

            // These are the heavy CPU operations (Perlin noise)
            _worldGenerator.GenerateChunkHeightmap(chunk);
            _worldGenerator.DecorateChunkHeightmap(chunk);

            // GeneratedChunkQueue is a ConcurrentQueue, so this is thread-safe
            _mailbox.ChunkQueue.Enqueue(chunk);
        });

        // 3. CLEANUP: Update ECS components on the Main Thread
        // We cannot do this inside Parallel.ForEach because CommandBuffer is not thread-safe
        foreach (var item in batch)
        {
            commandBuffer.Remove<NeedsGeneration>(item.Entity);
            commandBuffer.Add<NeedsMeshing>(item.Entity);
        }

        // Apply changes
        commandBuffer.Playback(_world);
    }
}