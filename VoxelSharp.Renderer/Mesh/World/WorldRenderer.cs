using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Renderer.Mesh.World;

public class WorldRenderer : IRenderer
{
    private readonly ICameraMatricesProvider _cameraMatricesProvider;
    private readonly ChunkMesh[] _chunkMeshArray;
    private Shader? _chunkShader;

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
        Console.WriteLine("Initializing shaders for WorldRenderer");
        _chunkShader = new Shader("Shaders/chunk.vert", "Shaders/chunk.frag");
    }


    public void Render(double interpolationFactor)
    {
        if (_chunkShader == null)
            throw new InvalidOperationException("Shaders not initialized. Call InitializeShaders before rendering.");

        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", _cameraMatricesProvider.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", _cameraMatricesProvider.GetProjectionMatrix());

        foreach (var chunkMesh in _chunkMeshArray) chunkMesh.Render(_chunkShader);

        Shader.Unuse();
    }
}