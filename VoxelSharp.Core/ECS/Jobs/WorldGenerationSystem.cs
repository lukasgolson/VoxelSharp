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


        _world.ParallelQuery(in query,
            (Entity entity, ref ChunkPosition pos, ref ChunkData data, ref NeedsGeneration _) =>
            {
                var chunk = new Chunk(new Position<int>(pos.X, pos.Y, pos.Z), _voxelWorld.ChunkSize);

                _worldGenerator.GenerateChunkHeightmap(chunk);

                _worldGenerator.DecorateChunkHeightmap(chunk);

                data.Chunk = chunk;

                _mailbox.ChunkQueue.Enqueue(chunk);


                commandBuffer.Remove<NeedsGeneration>(entity);
                commandBuffer.Add<NeedsMeshing>(entity);
            });


        commandBuffer.Playback(_world);
    }
}