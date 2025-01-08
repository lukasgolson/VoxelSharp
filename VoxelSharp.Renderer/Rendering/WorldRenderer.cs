using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Renderer.Rendering;

public class WorldRenderer : IRenderer
{
    private readonly ICameraMatricesProvider _cameraMatricesProvider;
    private ChunkMesh[]? _chunkMeshArray;
    private Shader? _chunkShader;

    private readonly ILogger _logger;

    public WorldRenderer(ICameraMatricesProvider cameraMatricesProvider, ILogger<WorldRenderer> logger)
    {
        _cameraMatricesProvider = cameraMatricesProvider;

        _logger = logger;
    }

    public void AssociateWorld(VoxelWorld voxelWorld)
    {
        var worldVolume = voxelWorld.WorldSize * voxelWorld.WorldSize * voxelWorld.WorldSize;
        _chunkMeshArray = new ChunkMesh[worldVolume];

        foreach (var chunk in voxelWorld.ChunkArray)
        {
            var chunkMesh = new ChunkMesh(chunk);

            _chunkMeshArray[chunk.Position.ToIndex(voxelWorld.WorldSize)] = chunkMesh;
        }
    }

    public void InitializeShaders()
    {
        _logger.LogInformation("Initializing shaders for WorldRenderer");
        _chunkShader = new Shader("Shaders/chunk.vert", "Shaders/chunk.frag");
    }


    public void Render(double interpolationFactor)
    {
        if (_chunkShader == null)
            throw new InvalidOperationException("Shaders not initialized. Call InitializeShaders before rendering.");

        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", _cameraMatricesProvider.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", _cameraMatricesProvider.GetProjectionMatrix());

        if (_chunkMeshArray != null)
            foreach (var chunkMesh in _chunkMeshArray)
                chunkMesh.Render(_chunkShader);

        Shader.Unuse();
    }
}