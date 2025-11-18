using System.Buffers;
using System.Collections.Concurrent;
using VoxelSharp.Core.World;

namespace VoxelSharp.Renderer.Rendering;

public struct MeshData
{
    public Chunk Chunk;

    // We pass ownership of these memory buffers to the main thread
    public IMemoryOwner<float> OpaqueVertices;
    public int OpaqueCount;
    public IMemoryOwner<float> TransparentVertices;
    public int TransparentCount;
}

public class GeneratedMeshQueue
{
    public readonly ConcurrentQueue<MeshData> Queue = new();
}