using Microsoft.Extensions.Logging;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;

namespace VoxelSharp.Core.WorldGen;

public class EmptyWorldGenerator : IWorldGenerator
{
    private readonly ILogger<EmptyWorldGenerator> _logger;
    
    public EmptyWorldGenerator(ILogger<EmptyWorldGenerator> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public bool GenerateChunk(Chunk chunk)
    {
        chunk.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red));
        
        chunk.IsDirty = true;
        
        _logger.LogInformation("Generated chunk at position {0}", chunk.Position);


        return true;
    }
}