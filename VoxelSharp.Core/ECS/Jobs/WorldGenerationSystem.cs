using Arch.Core;
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

/// <summary>
/// Tags a chunk whose generation job is currently running on a worker thread.
/// </summary>
public struct Generating;

public struct ChunkData
{
    public Chunk? Chunk;
};

/// <summary>
/// Generates requested chunks on worker threads, nearest to the world's load centre first.
/// Finished chunks are handed to the <see cref="GeneratedChunkQueue"/> to be committed on the main thread.
/// </summary>
public class WorldGenerationSystem : IUpdatable
{
    private readonly IWorldGenerator _worldGenerator;
    private readonly ILogger<WorldGenerationSystem> _logger;
    private readonly VoxelWorld _voxelWorld;


    private readonly GeneratedChunkQueue _mailbox;

    private readonly Arch.Core.World _world;

    private readonly QueryDescription _pendingQuery =
        new QueryDescription().WithAll<ChunkPosition, NeedsGeneration>();

    private readonly List<(Entity Entity, Position<int> Position)> _pendingChunks = [];

    // Leave a core free for the main thread
    private readonly int _maxConcurrentJobs = System.Math.Max(1, Environment.ProcessorCount - 1);
    private int _runningJobs;

    public WorldGenerationSystem(IGameLoop gameLoop, Arch.Core.World world, IWorldGenerator worldGenerator,
        ILogger<WorldGenerationSystem> logger, VoxelWorld voxelWorld, GeneratedChunkQueue mailbox)
    {
        _worldGenerator = worldGenerator;
        _logger = logger;
        _world = world;
        _voxelWorld = voxelWorld;
        _mailbox = mailbox;

        gameLoop.RegisterUpdateAction(this);
    }


    public void Update(double deltaTime)
    {
        var freeJobSlots = _maxConcurrentJobs - Volatile.Read(ref _runningJobs);
        if (freeJobSlots <= 0)
            return;

        _pendingChunks.Clear();
        _world.Query(in _pendingQuery, (Entity entity, ref ChunkPosition pos) =>
            _pendingChunks.Add((entity, new Position<int>(pos.X, pos.Y, pos.Z))));

        if (_pendingChunks.Count == 0)
            return;

        var loadCenter = _voxelWorld.LoadCenter;
        _pendingChunks.Sort((a, b) => DistanceSquared(a.Position, loadCenter)
            .CompareTo(DistanceSquared(b.Position, loadCenter)));

        foreach (var (entity, position) in _pendingChunks.Take(freeJobSlots))
        {
            _world.Remove<NeedsGeneration>(entity);
            _world.Add<Generating>(entity);

            Interlocked.Increment(ref _runningJobs);
            Task.Run(() => GenerateChunk(position));
        }
    }

    private void GenerateChunk(Position<int> position)
    {
        try
        {
            var chunk = new Chunk(position, _voxelWorld.ChunkSize);

            _worldGenerator.GenerateChunkHeightmap(chunk);

            _worldGenerator.DecorateChunkHeightmap(chunk);

            _mailbox.ChunkQueue.Enqueue(chunk);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate chunk at {ChunkPos}", position);
        }
        finally
        {
            Interlocked.Decrement(ref _runningJobs);
        }
    }

    private static int DistanceSquared(Position<int> a, Position<int> b)
    {
        var difference = a - b;
        return Position<int>.Dot(difference, difference);
    }
}
