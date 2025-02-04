using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Resources;

namespace ExampleMod.WorldGen;

public class BasicWorldGenerator : IWorldGenerator
{
    public bool GenerateChunk(Chunk chunk)
    {
        var chunkSpan = chunk.GetVoxelSpan();

        // Fill the chunk with voxels if it's the bottom layer

        if (chunk.Position.Y < 0)
        {
            for (var x = 0; x < chunk.ChunkSize; x++)
            for (var z = 0; z < chunk.ChunkSize; z++)
            for (var y = 0; y < chunk.ChunkSize; y++)
                chunkSpan[chunk.GetVoxelIndex(new Position<int>(x, y, z))] = new Voxel(new Rgba(0, 255, 0));
        }

        chunk.IsDirty = true;


        return true;
    }
}