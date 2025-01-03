using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer.Mesh.World;

public class WorldRenderer : IRenderer
{
    private readonly ChunkMesh[] _chunkMeshArray;
    private Shader? _chunkShader;

    private readonly ICameraMatricesProvider _cameraMatricesProvider;

    public WorldRenderer(Core.World.World world, ICameraMatricesProvider cameraMatricesProvider)
    {
        _cameraMatricesProvider = cameraMatricesProvider;

        var worldVolume = world.WorldSize * world.WorldSize * world.WorldSize;
        _chunkMeshArray = new ChunkMesh[worldVolume];

        foreach (var chunk in world.ChunkArray)
        {
            var chunkMesh = new ChunkMesh(chunk);

            _chunkMeshArray[chunk.Position.ToIndex(world.WorldSize)] = chunkMesh;
        }
    }

    public void InitializeShaders()
    {
        _chunkShader = new Shader("Shaders/chunk.vert", "Shaders/chunk.frag");
    }


    public void Render(double interpolationFactor)
    {
        if (_chunkShader == null)
        {
            throw new InvalidOperationException("Shaders not initialized. Call InitializeShaders before rendering.");
        }

        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", _cameraMatricesProvider.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", _cameraMatricesProvider.GetProjectionMatrix());

        foreach (var chunkMesh in _chunkMeshArray) chunkMesh.Render(_chunkShader);

        Shader.UnUse();
    }
}