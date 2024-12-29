using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer.Mesh.World;

public class WorldRenderer : IRenderer
{
    private readonly ChunkMesh[] _chunkMeshArray;
    private Shader? _chunkShader;


    public WorldRenderer(Core.World.World world)
    {
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


    public void Render(ICamera camera)
    {
        if (_chunkShader == null)
        {
            throw new InvalidOperationException("Shaders not initialized. Call InitializeShaders before rendering.");
        }

        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", camera.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", camera.GetProjectionMatrix());

        foreach (var chunkMesh in _chunkMeshArray) chunkMesh.Render(_chunkShader);

        Shader.UnUse();
    }
}