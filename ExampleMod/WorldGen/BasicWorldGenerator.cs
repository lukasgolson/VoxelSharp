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
                var globalPos = chunk.LocalToGlobalPosition(new Position<int>(x, 0, z));

                var noiseValueA = _noiseGeneratorA.Generate(globalPos.X, 0, globalPos.Z, scale: 0.05) + 1;
                var noiseValueB = _noiseGeneratorB.Generate(globalPos.X, 0, globalPos.Z, scale: 0.001) + 1;
                //var noiseValueC = _noiseGeneratorB.Generate(globalPos.X, 0, globalPos.Z, scale: 0.0001) + 1.5;

                noiseValueA *= 10;
                noiseValueB *= 40;

                var noiseValue = (noiseValueA + noiseValueB) / 2;

                int terrainHeight = (int)noiseValue;
                //noiseValue *= noiseValueC;


                // 2. Fill voxels based on that height
                for (int y = 0; y < chunk.ChunkSize; y++)
                {
                    var currentGlobalY = chunk.Position.Y * chunk.ChunkSize + y;

                    if (currentGlobalY <= terrainHeight)
                    {
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
                if (voxelPosition.Y <= 20)
                {
                    span[index] = new Voxel(new Rgba(0, 0, 255, 128)); // Set the water
                }
            }
            else
            {
                if (voxelPosition.Y <= 22)
                {
                    span[index] = new Voxel(new Rgba(203, 165, 96, 255)); // Set the Sand
                }
                else
                {
                    span[index] = new Voxel(Rgba.Green);
                }
            }
        }


        chunk.IsDirty = true;

        return true;
    }
}