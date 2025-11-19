using System.Numerics;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.ECS.Jobs;
using VoxelSharp.Core.Helpers;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Helpers;
using VoxelSharp.Renderer.Mesh.World;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace VoxelSharp.Renderer.Rendering;

public class WorldRenderer : IRenderer, IUpdatable
{
    private readonly Dictionary<Position<int>, ChunkMesh> _chunkMeshArray;
    private Shader _chunkShader;


    private VoxelWorld? _voxelWorld;
    public const int RenderDistance = 10;

    private readonly List<ChunkMesh> _renderList = new();
    private Vector3 _lastSortPosition;
    private const float SortThresholdSq = 16.0f;


    private readonly ILogger _logger;
    private readonly ICameraMatrices _cameraMatrices;
    private readonly ICameraParameters _cameraParameters;
    private readonly Arch.Core.World _ecsWorld;
    
    private readonly ILightSource _lightSource;

    private readonly GeneratedMeshQueue _meshQueue;


    public WorldRenderer(ICameraMatrices cameraMatrices, ICameraParameters cameraParameters,
        ILogger<WorldRenderer> logger,
        IGameLoop gameLoop, Arch.Core.World ecsWorld, ILightSource lightSource, GeneratedMeshQueue meshQueue)
    {
        gameLoop.RegisterRenderAction(this);
        gameLoop.RegisterUpdateAction(this);

        _cameraMatrices = cameraMatrices;
        _cameraParameters = cameraParameters;
        _logger = logger;
        
        _ecsWorld = ecsWorld;
        
        var worldVolume = Math.Pow(RenderDistance, 3);
        _chunkMeshArray = new Dictionary<Position<int>, ChunkMesh>((int)worldVolume);
        
        _lightSource = lightSource;

        _meshQueue = meshQueue;
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
        
        var sysLightDir = _lightSource.GetCurrentLightDirection();
        var sysLightColor = _lightSource.GetCurrentLightColor();
        
        var currentLightDirection = new Vector3(sysLightDir.X, sysLightDir.Y, sysLightDir.Z);
        var currentLightColor = new Vector3(sysLightColor.X, sysLightColor.Y, sysLightColor.Z);
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);


        var viewProj = _cameraMatrices.GetViewMatrix() * _cameraMatrices.GetProjectionMatrix();
        FrustumCuller.UpdateFrustum(viewProj);

        int chunkSize = _voxelWorld?.ChunkSize ?? 16;


        _chunkShader.Use();

        _chunkShader.SetUniform("m_view", _cameraMatrices.GetViewMatrix());
        _chunkShader.SetUniform("m_projection", _cameraMatrices.GetProjectionMatrix());
        _chunkShader.SetUniform("lightDirection", currentLightDirection);
        _chunkShader.SetUniform("lightColour", currentLightColor);


        // -- Pass 1: Opaque Geometry --
        GL.DepthMask(true);
        // No blending needed
        GL.Disable(EnableCap.Blend);

        foreach (var chunkMesh in _renderList)
        {
            if (chunkMesh.Chunk != null &&
                FrustumCuller.IsChunkVisible(chunkMesh.Chunk.Position, chunkSize))
            {
                chunkMesh.RenderOpaque(_chunkShader);
            }
        }


        // -- Pass 2: Transparent geometry --
        GL.Enable(EnableCap.Blend);
        // CRITICAL: Disable depth writing
        GL.DepthMask(false);

        var cameraPos = _cameraParameters.Position;


        var cameraVec3 = new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z);
        
        if (Vector3.DistanceSquared(_lastSortPosition, cameraVec3) > SortThresholdSq)
        {
            // Allocation-Free Sort using custom struct comparer
            _renderList.Sort(new ChunkDistanceComparer(cameraVec3, _voxelWorld.ChunkSize));
            _lastSortPosition = cameraVec3;
        }

        foreach (var chunkMesh in _renderList)
        {
            if (chunkMesh.Chunk != null &&
                FrustumCuller.IsChunkVisible(chunkMesh.Chunk.Position, chunkSize))
            {
                chunkMesh.RenderTransparent(_chunkShader);
            }
        }

    

        // Reset GL state
        GL.DepthMask(true);
        Shader.Unuse();
    }


    private float _updateTimer;

    public void Update(double deltaTime)
    {
        _updateTimer += (float)deltaTime;

        if (_updateTimer < 0.5f)
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


        while (_meshQueue.Queue.TryDequeue(out var meshData))
        {
            if (!_chunkMeshArray.TryGetValue(meshData.Chunk.Position, out var chunkMesh))
            {
                chunkMesh = new ChunkMesh(meshData.Chunk); // Removed VoxelWorld dependency
                _chunkMeshArray[meshData.Chunk.Position] = chunkMesh;
            }

            chunkMesh.UploadMeshData(meshData);
        }
        
        
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


        if (_renderList.Count != _chunkMeshArray.Count)
        {
            _renderList.Clear();
            _renderList.AddRange(_chunkMeshArray.Values);
            _lastSortPosition = new Vector3(float.MinValue); // Force resort
        }

        foreach (var chunkPos in chunkPositions)
        {
            if (_chunkMeshArray.ContainsKey(chunkPos))
                continue;
            
            var chunk = _voxelWorld.GetChunk(chunkPos);
            if (chunk == null)
            {
                _voxelWorld.RequestChunk(chunkPos);
            }
        }
    }
}