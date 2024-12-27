using VoxelSharp.Core.Renderer;
using VoxelSharp.Core.Renderer.Mesh.World;
using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.World;

public class World
{
    private int _worldSize;
    private int _chunkSize;


    private readonly Chunk[] _chunkArray;
    private readonly ChunkMesh[] _chunkMeshArray;


    public World(int worldSize, int chunkSize)
    {
        _worldSize = worldSize;
        _chunkSize = chunkSize;


        var worldVolume = _worldSize * _worldSize * _worldSize;

        _chunkArray = new Chunk[worldVolume];
        _chunkMeshArray = new ChunkMesh[worldVolume];


        for (int x = 0; x < _worldSize; ++x)
        {
            for (int z = 0; z < _worldSize; ++z)
            {
                for (int y = 0; y < _worldSize; ++y)
                {
                    var chunk = new Chunk(new Position<int>(x, y, z), _chunkSize);
                    var chunkMesh = new ChunkMesh(chunk);
                    
                    _chunkArray[new Position<int>(x, y, z).ToIndex(_worldSize)] = chunk;
                    _chunkMeshArray[new Position<int>(x, y, z).ToIndex(_worldSize)] = chunkMesh;
                }
            }
        }
    }


    public void Render(Shader shaderProgram)
    {
        foreach (var chunkMesh in _chunkMeshArray)
        {
            chunkMesh.Render(shaderProgram);
        }
    }


    public Voxel GetVoxel(Position<int> worldPos)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        Voxel voxel = _chunkArray[chunkCoords.ToIndex(_worldSize)].GetVoxel(localCoords);

        return voxel;
    }

    public bool SetVoxel(Position<int> worldPos, Voxel voxel)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        var success = _chunkArray[chunkCoords.ToIndex(_worldSize)].SetVoxel(localCoords, voxel);

        return success;
    }

    private Position<int> GetChunkCoordinates(Position<int> worldCoords)
    {
        int x = worldCoords.X / _chunkSize;
        int y = worldCoords.Y / _chunkSize;
        int z = worldCoords.Z / _chunkSize;

        return new Position<int>(x, y, z);
    }

    private Position<int> GetLocalCoordinates(Position<int> worldCoords)
    {
        int x = Math.EModulo(worldCoords.X, _chunkSize);
        int y = Math.EModulo(worldCoords.Y, _chunkSize);
        int z = Math.EModulo(worldCoords.Z, _chunkSize);

        return new Position<int>(x, y, z);
    }
}