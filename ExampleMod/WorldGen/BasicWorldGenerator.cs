using Microsoft.Extensions.Logging;
using VoxelSharp.Core.Helpers;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Resources;

namespace ExampleMod.WorldGen;

public class BasicWorldGenerator : IWorldGenerator
{
    private readonly PerlinNoiseGenerator _noiseGeneratorA = new();
    private readonly PerlinNoiseGenerator _noiseGeneratorB = new();

    ILogger<BasicWorldGenerator> _logger;

    public BasicWorldGenerator(ILogger<BasicWorldGenerator> logger)
    {
        _logger = logger;
    }


    public bool GenerateChunkHeightmap(Chunk chunk)
    {
        var chunkSpan = chunk.GetVoxelSpan();


        for (var x = 0; x < chunk.ChunkSize; x++)
        {
            for (int z = 0; z < chunk.ChunkSize; z++)
            {
                for (int y = 0; y < chunk.ChunkSize; y++)
                {
                    var globalPos = chunk.LocalToGlobalPosition(new Position<int>(x, y, z));

                    var noiseValueA = _noiseGeneratorA.Generate(globalPos.X, 0, globalPos.Z, scale: 0.05) + 1;
                    var noiseValueB = _noiseGeneratorB.Generate(globalPos.X, 0, globalPos.Z, scale: 0.001) + 1;
                    //var noiseValueC = _noiseGeneratorB.Generate(globalPos.X, 0, globalPos.Z, scale: 0.0001) + 1.5;

                    noiseValueA *= 10;
                    noiseValueB *= 40;

                    var noiseValue = (noiseValueA + noiseValueB) / 2;
                    //noiseValue *= noiseValueC;


                    if (globalPos.Y <= noiseValue)
                    {
                        // set voxel value to 1

                        var voxelPosition = new Position<int>(x, y, z);




                        chunkSpan[chunk.GetVoxelIndex(voxelPosition)] = new Voxel(Rgba.Black);
                    }
                }
            }
        }


        chunk.IsDirty = true;


        return true;
    }

    public bool DecorateChunkHeightmap(Chunk chunk)
    {
        var span = chunk.GetVoxelSpan();

        foreach (var (voxelPosition, index) in chunk.IterateVoxelPositions(globalPosition: true))
        {
            if (span[index].IsEmpty)
            {
                if (voxelPosition.Y <= 30)
                {
                    span[index] = new Voxel(new Rgba(0,0,255,125)); // Set the water
                }
            }
            else
            {
                span[index] = new Voxel(Rgba.Grey);
            }
        }


        chunk.IsDirty = true;

        return true;
    }
}