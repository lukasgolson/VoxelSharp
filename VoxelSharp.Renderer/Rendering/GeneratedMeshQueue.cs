using System.Buffers;
using System.Collections.Concurrent;
using VoxelSharp.Core.World;

namespace VoxelSharp.Renderer.Rendering;

public struct MeshData
{
    public Chunk Chunk;

    public int[] OpaqueVertices;
    public int OpaqueCount;
    public int[] TransparentVertices;
    public int TransparentCount;
}

public class GeneratedMeshQueue
{
    public readonly ConcurrentQueue<MeshData> Queue = new();
}