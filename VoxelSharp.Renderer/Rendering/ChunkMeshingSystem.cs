using Arch.Buffer;
using Arch.Core;
using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Core.ECS.Jobs;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Renderer.Rendering;

public class ChunkMeshingSystem : IUpdatable
{
    private readonly World _ecsWorld;
    private readonly VoxelWorld _voxelWorld;
    private readonly GeneratedMeshQueue _mailbox;
    private readonly ILogger<ChunkMeshingSystem> _logger;

    private const int MaxChunksPerFrame = 16;
    private const int MaxQueueSize = 32;

    public ChunkMeshingSystem(IGameLoop gameLoop, World ecsWorld, VoxelWorld voxelWorld,
        GeneratedMeshQueue mailbox, ILogger<ChunkMeshingSystem> logger)
    {
        _ecsWorld = ecsWorld;
        _voxelWorld = voxelWorld;
        _mailbox = mailbox;
        _logger = logger;

        // Register to run on background threads if supported, or main thread update
        gameLoop.RegisterUpdateAction(this);
    }

    public void Update(double deltaTime)
    {
        // 1. BACKPRESSURE: Check if the mailbox is full
        if (_mailbox.Queue.Count >= MaxQueueSize) return;

        using var commandBuffer = new CommandBuffer();

        int scheduledCount = 0;
        var dirtyQuery = new QueryDescription().WithAll<ChunkData>().WithNone<NeedsMeshing>();

        _ecsWorld.Query(in dirtyQuery, (Entity entity, ref ChunkData data) =>
        {
            // Stop if we hit our frame limit
            if (scheduledCount >= MaxChunksPerFrame) return;

            if (data.Chunk != null && data.Chunk.IsDirty)
            {
                commandBuffer.Add<NeedsMeshing>(entity);
                scheduledCount++;
            }
        });

        // Apply the 'NeedsMeshing' component to the chosen batch
        commandBuffer.Playback(_ecsWorld);

        // ==========================================================
        // 3. MESH CHUNKS (STANDARD PARALLELIZATION)
        // Bypass ParallelQuery to avoid JobScheduler thread crashes
        // ==========================================================
        var meshQuery = new QueryDescription().WithAll<ChunkData, NeedsMeshing>();
        var batch = new List<(Entity Entity, Core.World.Chunk Chunk)>();

        // Collect the batch synchronously
        _ecsWorld.Query(in meshQuery, (Entity entity, ref ChunkData chunkData) =>
        {
            if (chunkData.Chunk != null)
            {
                batch.Add((entity, chunkData.Chunk));
            }
        });

        // Process heavy lifting on standard ThreadPool
        Parallel.ForEach(batch, item =>
        {
            var chunk = item.Chunk;
            var chunkPos = chunk.Position;

            // Pre-fetch neighbors
            var cLeft = _voxelWorld.GetChunk(chunkPos - Position<int>.Right);
            var leftSpan = cLeft != null ? cLeft.GetVoxelSpan() : Span<Voxel>.Empty;

            var cRight = _voxelWorld.GetChunk(chunkPos + Position<int>.Right);
            var rightSpan = cRight != null ? cRight.GetVoxelSpan() : Span<Voxel>.Empty;

            var cDown = _voxelWorld.GetChunk(chunkPos - Position<int>.Up);
            var downSpan = cDown != null ? cDown.GetVoxelSpan() : Span<Voxel>.Empty;

            var cUp = _voxelWorld.GetChunk(chunkPos + Position<int>.Up);
            var upSpan = cUp != null ? cUp.GetVoxelSpan() : Span<Voxel>.Empty;

            var cBack = _voxelWorld.GetChunk(chunkPos - Position<int>.Forward);
            var backSpan = cBack != null ? cBack.GetVoxelSpan() : Span<Voxel>.Empty;

            var cFront = _voxelWorld.GetChunk(chunkPos + Position<int>.Forward);
            var frontSpan = cFront != null ? cFront.GetVoxelSpan() : Span<Voxel>.Empty;

            // Generate
            ChunkMesh.GenerateMesh(chunk, leftSpan, rightSpan, downSpan, upSpan, backSpan, frontSpan, out var meshData);

            // Send to Main Thread
            _mailbox.Queue.Enqueue(meshData);
        });

        // Cleanup ECS synchronously
        foreach (var item in batch)
        {
            commandBuffer.Remove<NeedsMeshing>(item.Entity);
        }

        commandBuffer.Playback(_ecsWorld);
    }
}

