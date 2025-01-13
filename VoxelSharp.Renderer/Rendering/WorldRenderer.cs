using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Renderer.Rendering;

public class WorldRenderer : IRenderer, IUpdatable
{
    private readonly Dictionary<Position<int>, ChunkMesh> _chunkMeshArray;
    private Shader? _chunkShader;

    private readonly Position<int> _currentRenderPosition = Position<int>.Zero;

    private VoxelWorld? _voxelWorld;

    private const int RenderDistance = 4;
    private const int RenderDistanceSquared = RenderDistance * RenderDistance;

    private readonly ILogger _logger;
    private readonly ICameraMatricesProvider _cameraMatricesProvider;

    public WorldRenderer(ICameraMatricesProvider cameraMatricesProvider, ILogger<WorldRenderer> logger,
        IGameLoop gameLoop)
    {
        gameLoop.RegisterRenderAction(this);
        gameLoop.RegisterUpdateAction(this);

        _cameraMatricesProvider = cameraMatricesProvider;
        _logger = logger;

        var worldVolume = Math.Pow(RenderDistance, 3);
        _chunkMeshArray = new Dictionary<Position<int>, ChunkMesh>((int)worldVolume);
    }

    public void AssociateWorld(VoxelWorld voxelWorld)
    {
        _voxelWorld = voxelWorld;
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

        foreach (var chunkMesh in _chunkMeshArray.Values)
            chunkMesh.Render(_chunkShader);

        Shader.Unuse();
    }


    private float _updateTimer;

    public void Update(double deltaTime)
    {
        _updateTimer += (float)deltaTime;

        if (_updateTimer < 0.25f)
            return;

        _updateTimer = 0;

        // based on the camera position, determine which chunks to render


        if (_voxelWorld == null)
            throw new InvalidOperationException("VoxelWorld not associated with WorldRenderer.");

        if (_chunkMeshArray == null)
            throw new InvalidOperationException("ChunkMesh array not initialized.");


        // the list of chunks to render
        List<Position<int>> chunkPositions = [];

        for (var x = -RenderDistance; x < RenderDistance; x++)
        {
            for (var y = -RenderDistance; y < RenderDistance; y++)
            {
                for (var z = -RenderDistance; z < RenderDistance; z++)
                {
                    if (x * x + y * y + z * z >= RenderDistanceSquared) continue;

                    var chunkPos = new Position<int>(
                        _currentRenderPosition.X + x,
                        _currentRenderPosition.Y + y,
                        _currentRenderPosition.Z + z
                    );

                    if (_voxelWorld.IsChunkLoaded(chunkPos))
                    {
                        chunkPositions.Add(chunkPos);
                    }
                }
            }
        }


        // remove chunks that are no longer in the render distance
        var keysToRemove = _chunkMeshArray.Keys.Except(chunkPositions).ToList();
        foreach (var key in keysToRemove)
        {
            _chunkMeshArray[key].Dispose();
            _chunkMeshArray.Remove(key);
            _logger.LogInformation("Removed chunk at {ChunkPos}", key);
        }

        // add new chunks that are in the render distance
        foreach (var chunkPos in chunkPositions)
        {
            if (!_chunkMeshArray.ContainsKey(chunkPos))
            {
                var chunk = _voxelWorld.GetChunk(chunkPos);
                var chunkMesh = new ChunkMesh(chunk);
                _chunkMeshArray.Add(chunkPos, chunkMesh);

                _logger.LogInformation("Added chunk at {ChunkPos}", chunkPos);
            }
        }
    }
}