using System.Collections.Concurrent;
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

    private float _updateTimer;

    
    // Add these fields to the top of WorldRenderer
    private bool _needsChunkUpdate;
    private Position<int> _currentRenderPosition;

   // Add this field to the top of WorldRenderer:
    private readonly ConcurrentQueue<ChunkMesh> _disposalQueue = new();
    private readonly object _listLock = new object(); // Protects _renderList

    public void Update(double deltaTime)
    {
        _updateTimer += (float)deltaTime;
        if (_updateTimer < 0.5f) return;
        _updateTimer = 0;

        if (_voxelWorld == null || _chunkMeshArray == null) return;

        var currentCameraPosition = _cameraParameters.Position.AsPosition();
        var currentRenderPosition = _voxelWorld.GetChunkCoordinates(currentCameraPosition.RoundToInt());
        
        HashSet<Position<int>> chunkPositions = [];

        for (var x = -RenderDistance; x < RenderDistance; x++)
        {
            for (var y = -RenderDistance; y < RenderDistance; y++)
            {
                for (var z = -RenderDistance; z < RenderDistance; z++)
                {
                    chunkPositions.Add(new Position<int>(
                        currentRenderPosition.X + x,
                        currentRenderPosition.Y + y,
                        currentRenderPosition.Z + z
                    ));
                }
            }
        }

        // 1. Safely remove chunks out of range
        var keysToRemove = _chunkMeshArray.Keys.Except(chunkPositions).ToList();
        foreach (var key in keysToRemove)
        {
            // Don't dispose here! Send it to the UI thread.
            _disposalQueue.Enqueue(_chunkMeshArray[key]);
            _chunkMeshArray.Remove(key);
        }

        // 2. Safely rebuild the render list
        lock (_listLock)
        {
            if (_renderList.Count != _chunkMeshArray.Count)
            {
                _renderList.Clear();
                _renderList.AddRange(_chunkMeshArray.Values);
                _lastSortPosition = new Vector3(float.MinValue); // Force resort
            }
        }

        // 3. Request missing chunks
        foreach (var chunkPos in chunkPositions)
        {
            if (!_chunkMeshArray.ContainsKey(chunkPos))
            {
                if (_voxelWorld.GetChunk(chunkPos) == null)
                {
                    _voxelWorld.RequestChunk(chunkPos);
                }
            }
        }
    }

    public void Render(double interpolationFactor)
    {
        if (!_initialized)
        {
            InitializeShaders();
            if (_chunkShader == null) throw new InvalidOperationException("Chunk shader not initialized.");
            _initialized = true;
        }

        // ====================================================================
        // THREAD-SAFE OPENGL EXECUTIONS
        // ====================================================================
        
        // 1. Process Uploads (Safe because we are on the UI/Render thread)
        while (_meshQueue.Queue.TryDequeue(out var meshData))
        {
            if (!_chunkMeshArray.TryGetValue(meshData.Chunk.Position, out var chunkMesh))
            {
                chunkMesh = new ChunkMesh(meshData.Chunk);
                _chunkMeshArray[meshData.Chunk.Position] = chunkMesh;
                
                lock (_listLock) { _renderList.Add(chunkMesh); }
            }
            chunkMesh.UploadMeshData(meshData); 
        }

        // 2. Process Disposals (Safe because we are on the UI/Render thread)
        while (_disposalQueue.TryDequeue(out var oldMesh))
        {
            oldMesh.Dispose();
        }

        // ====================================================================
        // STANDARD RENDERING
        // ====================================================================
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

        // Lock the list while drawing so the background thread doesn't clear it mid-frame
        lock (_listLock)
        {
            // -- Pass 1: Opaque Geometry --
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

            foreach (var chunkMesh in _renderList)
            {
                if (chunkMesh.Chunk != null && FrustumCuller.IsChunkVisible(chunkMesh.Chunk.Position, chunkSize))
                {
                    chunkMesh.RenderOpaque(_chunkShader);
                }
            }

            // -- Pass 2: Transparent geometry --
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);

            var cameraPos = _cameraParameters.Position;
            var cameraVec3 = new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z);
            
            if (Vector3.DistanceSquared(_lastSortPosition, cameraVec3) > SortThresholdSq)
            {
                _renderList.Sort(new ChunkDistanceComparer(cameraVec3, chunkSize));
                _lastSortPosition = cameraVec3;
            }

            foreach (var chunkMesh in _renderList)
            {
                if (chunkMesh.Chunk != null && FrustumCuller.IsChunkVisible(chunkMesh.Chunk.Position, chunkSize))
                {
                    chunkMesh.RenderTransparent(_chunkShader);
                }
            }
        }

        GL.DepthMask(true);
        Shader.Unuse();
    }
}