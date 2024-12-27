using VoxelSharp.Core.Renderer;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer.Mesh.World;

public class WorldRenderer : IRenderable
{
    private readonly ChunkMesh[] _chunkMeshArray;


    private WorldRenderer(Core.World.World world)
    {
        var worldVolume = world.WorldSize * world.WorldSize * world.WorldSize;
        _chunkMeshArray = new ChunkMesh[worldVolume];

        foreach (var chunk in world.ChunkArray)
        {
            var chunkMesh = new ChunkMesh(chunk);

            _chunkMeshArray[chunk.Position.ToIndex(world.WorldSize)] = chunkMesh;
        }
    }


    public void Render(Shader shaderProgram)
    {
        foreach (var chunkMesh in _chunkMeshArray)
        {
            chunkMesh.Render(shaderProgram);
        }
    }
}