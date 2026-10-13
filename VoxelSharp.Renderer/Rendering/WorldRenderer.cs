using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.ECS.Jobs;
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

    // Time per frame spent building chunk meshes, so a wave of newly loaded chunks
    // is spread over several frames instead of causing a hitch
    private static readonly TimeSpan MeshBuildBudget = TimeSpan.FromMilliseconds(4);

    private readonly List<ChunkMesh> _meshesToBuild = [];


    private readonly Vector3 _lightDirection = new(0.5f, -0.8f, 0.3f);

    private readonly ILogger _logger;
    private readonly ICameraMatrices _cameraMatrices;
    private readonly ICameraParameters _cameraParameters;
    private readonly Arch.Core.World _ecsWorld;


    public WorldRenderer(ICameraMatrices cameraMatrices, ICameraParameters cameraParameters,
        ILogger<WorldRenderer> logger,
        IGameLoop gameLoop, Arch.Core.World ecsWorld)
    {
        gameLoop.RegisterRenderAction(this);
        gameLoop.RegisterUpdateAction(this);

        _cameraMatrices = cameraMatrices;
        _cameraParameters = cameraParameters;
        _logger = logger;
        
        _ecsWorld = ecsWorld;
        
        var worldVolume = Math.Pow(RenderDistance * 2 + 1, 3);
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
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);


        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", _cameraMatrices.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", _cameraMatrices.GetProjectionMatrix());
        _chunkShader.SetUniform("lightDirection", _lightDirection);

        BuildChunkMeshes();


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
                    mesh.Chunk.Position.X,
                    mesh.Chunk.Position.Y,
                    mesh.Chunk.Position.Z) * mesh.Chunk.ChunkSize,
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


    /// <summary>
    ///     Builds dirty or new chunk meshes nearest the camera first, until the frame's budget is used up.
    ///     At least one mesh is built each frame so progress is always made.
    /// </summary>
    private void BuildChunkMeshes()
    {
        _meshesToBuild.Clear();
        _meshesToBuild.AddRange(_chunkMeshArray.Values.Where(mesh => mesh.NeedsRebuild));

        if (_meshesToBuild.Count == 0)
            return;

        var cameraPosition = _cameraParameters.Position;
        _meshesToBuild.Sort((a, b) => DistanceSquaredToCamera(a, cameraPosition)
            .CompareTo(DistanceSquaredToCamera(b, cameraPosition)));

        var startTime = Stopwatch.GetTimestamp();

        foreach (var mesh in _meshesToBuild)
        {
            mesh.Rebuild(_chunkShader);

            if (Stopwatch.GetElapsedTime(startTime) > MeshBuildBudget)
                break;
        }
    }

    private static float DistanceSquaredToCamera(ChunkMesh mesh, System.Numerics.Vector3 cameraPosition)
    {
        var chunkCenter = (new System.Numerics.Vector3(mesh.Chunk.Position.X, mesh.Chunk.Position.Y,
            mesh.Chunk.Position.Z) + new System.Numerics.Vector3(0.5f)) * mesh.Chunk.ChunkSize;

        return System.Numerics.Vector3.DistanceSquared(chunkCenter, cameraPosition);
    }


    private float _updateTimer;

    public void Update(double deltaTime)
    {
        _updateTimer += (float)deltaTime;

        if (_updateTimer < 0.1f)
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

        // Load chunks around the camera and unload those that are now out of range
        _voxelWorld.UpdateLoadedRegion(currentRenderPosition, RenderDistance);


        // remove meshes that are out of render distance, or whose chunk has since been unloaded
        var keysToRemove = _chunkMeshArray
            .Where(pair => VoxelWorld.ChunkDistance(pair.Key, currentRenderPosition) > RenderDistance
                           || _voxelWorld.GetChunk(pair.Key) != pair.Value.Chunk)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _chunkMeshArray[key].Dispose();
            _chunkMeshArray.Remove(key);
        }

        // create meshes for chunks that have finished generating
        for (var x = -RenderDistance; x <= RenderDistance; x++)
        for (var y = -RenderDistance; y <= RenderDistance; y++)
        for (var z = -RenderDistance; z <= RenderDistance; z++)
        {
            var chunkPos = currentRenderPosition + new Position<int>(x, y, z);

            if (_chunkMeshArray.ContainsKey(chunkPos))
                continue;

            var chunk = _voxelWorld.GetChunk(chunkPos);
            if (chunk != null)
                _chunkMeshArray.Add(chunkPos, new ChunkMesh(chunk, _voxelWorld));
        }
    }
}
