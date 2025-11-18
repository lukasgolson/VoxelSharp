using VoxelSegmentation.structs;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Resources;

namespace VoxelSegmentation;

public class PointcloudImporter : IUpdatable
{
    private readonly VoxelWorld _voxelWorld;
    
    // Queue to hold file paths requested by the UI
    private readonly Queue<string> _importQueue = new();
    
    // Store pending updates grouped by Chunk Coordinate
    private Dictionary<Position<int>, List<(int Index, Voxel Voxel)>>? _pendingChunks;
    
    // Track if we are currently processing a file
    private bool _isProcessing = false;

    public PointcloudImporter(VoxelWorld world)
    {
        _voxelWorld = world;
    }

    /// <summary>
    /// Called by the UI to start an import.
    /// </summary>
    public void QueueImport(string filePath)
    {
        if (File.Exists(filePath))
        {
            _importQueue.Enqueue(filePath);
            Console.WriteLine($"Queued import for: {filePath}");
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }
    }

    public void Update(double deltaTime)
    {
        // 1. Check if we need to start a new import
        if (!_isProcessing && _importQueue.Count > 0)
        {
            var file = _importQueue.Dequeue();
            _isProcessing = true;
            // Run IO on a background thread to keep the UI responsive
            Task.Run(() => LoadAndSortPoints(file)); 
            return;
        }

        // 2. If we have pending chunks data, process it into the world
        if (_pendingChunks != null && _pendingChunks.Count > 0)
        {
            ProcessPendingChunks();
        }
    }

    private void ProcessPendingChunks()
    {
        List<Position<int>> finishedChunks = new();

        foreach (var chunkBatch in _pendingChunks)
        {
            var chunkPos = chunkBatch.Key;
            
            if (_voxelWorld.IsChunkLoaded(chunkPos))
            {
                var chunk = _voxelWorld.GetChunk(chunkPos);
                if (chunk != null)
                {
                    var span = chunk.GetVoxelSpan();
                    foreach (var update in chunkBatch.Value)
                    {
                        if(update.Index >= 0 && update.Index < span.Length)
                            span[update.Index] = update.Voxel;
                    }

                    chunk.IsDirty = true;
                    MarkNeighborsDirty(chunkPos);
                    finishedChunks.Add(chunkPos);
                }
            }
            else
            {
                _voxelWorld.RequestChunk(chunkPos);
            }
        }

        foreach (var pos in finishedChunks)
        {
            _pendingChunks.Remove(pos);
        }
        
        // If we cleared the buffer, we are done processing
        if (_pendingChunks.Count == 0)
        {
            _isProcessing = false;
            _pendingChunks = null;
            Console.WriteLine("Import Complete.");
        }
    }

    private void LoadAndSortPoints(string filePath)
    {
        Console.WriteLine($"Starting import for {filePath}...");
        
        var pcl = new Pointcloud(filePath);
        var quantized = pcl.Quantize(1.0f, centerAtOrigin: true); 

        var newBatch = new Dictionary<Position<int>, List<(int, Voxel)>>();
        int chunkSize = _voxelWorld.ChunkSize;
        int chunkArea = chunkSize * chunkSize;

        foreach (var point in quantized)
        {
            var worldPos = new Position<int>((int)point.X, (int)point.Y, (int)point.Z);
            var chunkPos = _voxelWorld.GetChunkCoordinates(worldPos);
            
            int lx = VoxelSharp.Core.Math.EModulo(worldPos.X, chunkSize);
            int ly = VoxelSharp.Core.Math.EModulo(worldPos.Y, chunkSize);
            int lz = VoxelSharp.Core.Math.EModulo(worldPos.Z, chunkSize);

            int flatIndex = lx + (chunkSize * lz) + (chunkArea * ly);
            var voxel = new Voxel(new Rgba((byte)point.R, (byte)point.G, (byte)point.B, 255));

            if (!newBatch.TryGetValue(chunkPos, out var list))
            {
                list = new List<(int, Voxel)>();
                newBatch[chunkPos] = list;
            }
            list.Add((flatIndex, voxel));
        }
        
        // Assign to the main field so Update() can pick it up
        _pendingChunks = newBatch;
        Console.WriteLine($"Data prepared. Applying to world...");
    }

    private void MarkNeighborsDirty(Position<int> center)
    {
        MarkDirtyIfLoaded(center + Position<int>.Right);
        MarkDirtyIfLoaded(center - Position<int>.Right);
        MarkDirtyIfLoaded(center + Position<int>.Up);
        MarkDirtyIfLoaded(center - Position<int>.Up);
        MarkDirtyIfLoaded(center + Position<int>.Forward);
        MarkDirtyIfLoaded(center - Position<int>.Forward);
    }

    private void MarkDirtyIfLoaded(Position<int> pos)
    {
        if (_voxelWorld.IsChunkLoaded(pos))
        {
            var c = _voxelWorld.GetChunk(pos);
            if(c != null) c.IsDirty = true;
        }
    }
}