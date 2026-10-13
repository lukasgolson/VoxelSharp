using System.Collections.Concurrent;

namespace VoxelSharp.Core.World;

public class GeneratedChunkQueue
{
    public ConcurrentQueue<Chunk> ChunkQueue = new();
}