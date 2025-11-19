

using OpenTK.Mathematics;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Renderer.Helpers;

public readonly struct ChunkDistanceComparer : IComparer<ChunkMesh>
{
    private readonly Vector3 _cameraPos;
    private readonly int _chunkSize; 

    public ChunkDistanceComparer(Vector3 cameraPos, int chunkSize)
    {
        _cameraPos = cameraPos;
        _chunkSize = chunkSize;
    }

    public int Compare(ChunkMesh? x, ChunkMesh? y)
    {
        if (x == null || y == null) return 0;

        Vector3 xPos = new Vector3(x.Chunk!.Position.X, x.Chunk.Position.Y, x.Chunk.Position.Z) * _chunkSize;
        Vector3 yPos = new Vector3(y.Chunk!.Position.X, y.Chunk.Position.Y, y.Chunk.Position.Z) * _chunkSize;

        float xDist = Vector3.DistanceSquared(xPos, _cameraPos);
        float yDist = Vector3.DistanceSquared(yPos, _cameraPos);

        return yDist.CompareTo(xDist);
    }
}