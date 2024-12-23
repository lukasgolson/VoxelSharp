using VoxelSharp.Mesh;
using VoxelSharp.Structs;

namespace VoxelSharp.World;

public class World
{
    int worldSize;
    int worldVolume;
    int chunkSize;


    World(int worldSize, int chunkSize)
    {
    }

    Chunk[] chunkArray;
    ChunkMesh[] chunkMeshArray;

    void Render(Shader shaderProgram)
    {
    }

    void SetVoxel(Position<int> worldPos, Voxel voxel)
    {
    }

    Position<int> GetChunkCoordinates(Position<int> worldCoords)
    {
        throw new System.NotImplementedException();
        return new Position<int>(0, 0, 0);
    }

    Voxel GetVoxel(Position<int> worldPos)
    {
        throw new System.NotImplementedException();
        return new Voxel(new Color(0, 0, 0, 0));
    }
}