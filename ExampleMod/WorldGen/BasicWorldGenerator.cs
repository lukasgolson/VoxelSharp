using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Resources;

namespace ExampleMod.WorldGen;

public class BasicWorldGenerator : IWorldGenerator
{
    public bool GenerateChunk(Chunk chunk)
    {
        
        var voxel = new Voxel(Rgba.Grey);


        var chunkSpan = chunk.GetVoxelSpan();
        
        // Fill the chunk with voxels if it's the bottom layer
        
        if (chunk.Position.Y < 0)
        {
            for (var index = 0; index < chunkSpan.Length; index++)
            {
                chunkSpan[index] = voxel;
            }
        }
        
        chunk.IsDirty = true;
        
       
        
        
        
        

        return true;
    }
}