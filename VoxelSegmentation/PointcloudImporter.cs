using System.Numerics;
using VoxelSegmentation.structs;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Resources;

namespace VoxelSegmentation;

public class PointcloudImporter : IUpdatable
{
    private readonly VoxelWorld _voxelWorld;
    
    // Update queue to store parameters
    private readonly Queue<(string Path, Vector3 Rotation, float VoxelSize)> _importQueue = new();
    
    private Dictionary<Position<int>, List<(int Index, Voxel Voxel)>>? _pendingChunks;
    
    // State for UI
    public bool IsProcessing { get; private set; }
    public float ImportProgress { get; private set; }
    public string StatusMessage { get; private set; } = "";

    public PointcloudImporter(VoxelWorld world)
    {
        _voxelWorld = world;
    }

    public void QueueImport(string filePath, Vector3 rotation, float voxelSize)
    {
        if (File.Exists(filePath))
        {
            _importQueue.Enqueue((filePath, rotation, voxelSize));
            Console.WriteLine($"Queued import for: {filePath}");
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }
    }

    public void Update(double deltaTime)
    {
        if (!IsProcessing && _importQueue.Count > 0)
        {
            var request = _importQueue.Dequeue();
            IsProcessing = true;
            ImportProgress = 0f;
            StatusMessage = "Starting...";
            
            Task.Run(() => LoadAndSortPoints(request.Path, request.Rotation, request.VoxelSize)); 
            return;
        }

        if (_pendingChunks != null && _pendingChunks.Count > 0)
        {
            ProcessPendingChunks();
        }
    }

    private void ProcessPendingChunks()
    {
        StatusMessage = "Updating World...";
        
        // Simple progress based on chunks remaining
        // Note: This isn't perfect linear progress but gives feedback
        int totalChunks = _pendingChunks.Count; 
        // We don't know original total here easily without storing it, 
        // but we can keep the progress bar filled or pulsating.
        // Or we can let the 'Load' phase be 0-90% and this be 90-100%.
        ImportProgress = 0.9f + (0.1f * (1.0f - (_pendingChunks.Count / (float)(totalChunks + 1))));

        List<Position<int>> finishedChunks = new();
        int chunksProcessedThisFrame = 0;
        int maxChunksPerFrame = 16; // Throttle to prevent freezing

        foreach (var chunkBatch in _pendingChunks)
        {
            if (chunksProcessedThisFrame >= maxChunksPerFrame) break;

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
                    chunksProcessedThisFrame++;
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
        
        if (_pendingChunks.Count == 0)
        {
            IsProcessing = false;
            _pendingChunks = null;
            ImportProgress = 1.0f;
            StatusMessage = "Complete";
            Console.WriteLine("Import Complete.");
        }
    }

    private void LoadAndSortPoints(string filePath, Vector3 rotation, float voxelSize)
    {
        Console.WriteLine($"Starting import for {filePath}...");
        StatusMessage = "Reading File...";
        ImportProgress = 0f;

        // 1. Load (0% - 40%)
        var pcl = new Pointcloud(filePath, ' ', progress =>
        {
            ImportProgress = 0.0f + (progress * 0.4f);
        });

        // 2. Rotate
        if (rotation != Vector3.Zero)
        {
            StatusMessage = "Rotating...";
            pcl.Rotate(rotation);
        }

        // 3. Quantize (40% - 80%)
        StatusMessage = "Quantizing...";
        ImportProgress = 0.4f;
        
        // Note: Quantize is monolithic, so we just jump to 80% after it's done
        var quantized = pcl.Quantize(voxelSize, centerAtOrigin: true); 
        ImportProgress = 0.8f;

        StatusMessage = "Preparing Chunks...";
        var newBatch = new Dictionary<Position<int>, List<(int, Voxel)>>();
        int chunkSize = _voxelWorld.ChunkSize;
        int chunkArea = chunkSize * chunkSize;

        int count = 0;
        foreach (var point in quantized)
        {
            // Slight progress update during this loop
            count++;
            if (count % 1000 == 0)
            {
                ImportProgress = 0.8f + (0.1f * ((float)count / quantized.Count));
            }

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
        
        _pendingChunks = newBatch;
        ImportProgress = 0.9f;
        Console.WriteLine($"Data prepared. Applying to world...");
    }

    private void MarkNeighborsDirty(Position<int> center)
    {
        // (Keep existing implementation)
        MarkDirtyIfLoaded(center + Position<int>.Right);
        MarkDirtyIfLoaded(center - Position<int>.Right);
        MarkDirtyIfLoaded(center + Position<int>.Up);
        MarkDirtyIfLoaded(center - Position<int>.Up);
        MarkDirtyIfLoaded(center + Position<int>.Forward);
        MarkDirtyIfLoaded(center - Position<int>.Forward);
    }

    private void MarkDirtyIfLoaded(Position<int> pos)
    {
        // (Keep existing implementation)
        if (_voxelWorld.IsChunkLoaded(pos))
        {
            var c = _voxelWorld.GetChunk(pos);
            if(c != null) c.IsDirty = true;
        }
    }
}