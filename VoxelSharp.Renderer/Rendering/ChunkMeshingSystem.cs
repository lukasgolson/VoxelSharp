
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
    private readonly Arch.Core.World _ecsWorld;
    private readonly VoxelWorld _voxelWorld;
    private readonly GeneratedMeshQueue _mailbox;
    private readonly ILogger<ChunkMeshingSystem> _logger;

    public ChunkMeshingSystem(IGameLoop gameLoop, Arch.Core.World ecsWorld, VoxelWorld voxelWorld, 
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
        // 1. Find chunks that need meshing
        var query = new QueryDescription().WithAll<ChunkData, NeedsMeshing>();
        using var commandBuffer = new CommandBuffer();

        // 2. Run in parallel
        _ecsWorld.ParallelQuery(in query, (Entity entity, ref ChunkData chunkData) =>
        {
            var chunk = chunkData.Chunk;
            if (chunk == null) return;


            // 3. Pre-fetch neighbors (Safe for reading)
            var chunkPos = chunk.Position;

            // Fix: Use explicit null checks instead of '?.' for Spans
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

            // 4. Single-Pass Generation
            ChunkMesh.GenerateMesh(chunk, leftSpan, rightSpan, downSpan, upSpan, backSpan, frontSpan, out var meshData);

   

            // 5. Send to Main Thread
            _mailbox.Queue.Enqueue(meshData);

            // 6. Mark done
            commandBuffer.Remove<NeedsMeshing>(entity);
        });

        commandBuffer.Playback(_ecsWorld);
    }
}