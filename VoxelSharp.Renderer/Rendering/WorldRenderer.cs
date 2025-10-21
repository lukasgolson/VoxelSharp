using System.Numerics;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Helpers;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Mesh.World;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace VoxelSharp.Renderer.Rendering;

public class WorldRenderer : IRenderer, IUpdatable
{
    private readonly Dictionary<Position<int>, ChunkMesh> _chunkMeshArray;
    private Shader _chunkShader;


    private VoxelWorld? _voxelWorld;

    private const int RenderDistance = 4;


    private readonly Vector3 _lightDirection = new(0.5f, -0.8f, 0.3f);

    private readonly ILogger _logger;
    private readonly ICameraMatrices _cameraMatrices;
    private readonly ICameraParameters _cameraParameters;


    public WorldRenderer(ICameraMatrices cameraMatrices, ICameraParameters cameraParameters,
        ILogger<WorldRenderer> logger,
        IGameLoop gameLoop)
    {
        gameLoop.RegisterRenderAction(this);
        gameLoop.RegisterUpdateAction(this);

        _cameraMatrices = cameraMatrices;
        _cameraParameters = cameraParameters;
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


    private bool _initialized;

    public void Render(double interpolationFactor)
    {
        if (!_initialized)
        {
            InitializeShaders();

            if (_chunkShader == null)
                throw new InvalidOperationException("Chunk shader not initialized.");

            _initialized = true;
        }


        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", _cameraMatrices.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", _cameraMatrices.GetProjectionMatrix());
        _chunkShader.SetUniform("lightDirection", _lightDirection);


        // -- Pass 1: Opaque Geometry --
        GL.DepthMask(true);
        // No blending needed
        GL.Disable(EnableCap.Blend);

        foreach (var chunkMesh in _chunkMeshArray.Values)
        {
            chunkMesh.RenderOpaque(_chunkShader);
        }


        // -- Pass 2: Transparent geometry --
        GL.Enable(EnableCap.Blend);
        // CRITICAL: Disable depth writing
        GL.DepthMask(false);

        var cameraPos = _cameraParameters.Position;
        var cameraSNNVector = new System.Numerics.Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z);

        var sortedChunks = _chunkMeshArray.Values.OrderByDescending(mesh =>
            System.Numerics.Vector3.Distance(
                new System.Numerics.Vector3(
                    mesh._chunk.Position.X,
                    mesh._chunk.Position.Y,
                    mesh._chunk.Position.Z) * mesh._chunk.ChunkSize,
                cameraSNNVector
            )
        );

        foreach (var chunkMesh in sortedChunks)
        {
            chunkMesh.RenderTransparent(_chunkShader);
        }

        // --- CLEANUP ---

        // Reset GL state
        GL.DepthMask(true);
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


        var currentCameraPosition = _cameraParameters.Position.AsPosition();


        // convert the camera position to chunk position
        var currentRenderPosition = _voxelWorld.GetChunkCoordinates(currentCameraPosition.RoundToInt());

        // the list of chunks to render
        HashSet<Position<int>> chunkPositions = [];

        for (var x = -RenderDistance; x < RenderDistance; x++)
        {
            for (var y = -RenderDistance; y < RenderDistance; y++)
            {
                for (var z = -RenderDistance; z < RenderDistance; z++)
                {
                    var chunkPos = new Position<int>(
                        currentRenderPosition.X + x,
                        currentRenderPosition.Y + y,
                        currentRenderPosition.Z + z
                    );


                    chunkPositions.Add(chunkPos);
                }
            }
        }


        // remove chunks that are no longer in the render distance
        var keysToRemove = _chunkMeshArray.Keys.Except(chunkPositions).ToList();
        foreach (var key in keysToRemove)
        {
            _chunkMeshArray[key].Dispose();
            _chunkMeshArray.Remove(key);
        }

        // add new chunks that are in the render distance
        foreach (var chunkPos in chunkPositions)
        {
            if (_chunkMeshArray.ContainsKey(chunkPos)) continue;

            var chunk = _voxelWorld.GetChunk(chunkPos);
            var chunkMesh = new ChunkMesh(chunk);
            _chunkMeshArray.Add(chunkPos, chunkMesh);
        }
    }
}