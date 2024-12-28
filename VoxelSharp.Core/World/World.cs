﻿using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.World;

public class World
{
    public readonly Chunk[] ChunkArray;


    public World(int worldSize, int chunkSize)
    {
        WorldSize = worldSize;
        ChunkSize = chunkSize;


        var worldVolume = WorldSize * WorldSize * WorldSize;

        ChunkArray = new Chunk[worldVolume];

        for (var i = 0; i < worldVolume; i++)
        {
            var chunk = new Chunk(Position<int>.FromIndex(i, WorldSize), ChunkSize);
            ChunkArray[i] = chunk;
        }
    }

    public int WorldSize { get; }
    public int ChunkSize { get; }


    public Voxel GetVoxel(Position<int> worldPos)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        var voxel = ChunkArray[chunkCoords.ToIndex(WorldSize)].GetVoxel(localCoords);

        return voxel;
    }

    public void SetVoxel(Position<int> worldPos, Voxel voxel)
    {
        var chunkCoords = GetChunkCoordinates(worldPos);
        var localCoords = GetLocalCoordinates(worldPos);

        var success = ChunkArray[chunkCoords.ToIndex(WorldSize)].SetVoxel(localCoords, voxel);
    }

    private Position<int> GetChunkCoordinates(Position<int> worldCoords)
    {
        var x = worldCoords.X / ChunkSize;
        var y = worldCoords.Y / ChunkSize;
        var z = worldCoords.Z / ChunkSize;

        return new Position<int>(x, y, z);
    }

    private Position<int> GetLocalCoordinates(Position<int> worldCoords)
    {
        var x = Math.EModulo(worldCoords.X, ChunkSize);
        var y = Math.EModulo(worldCoords.Y, ChunkSize);
        var z = Math.EModulo(worldCoords.Z, ChunkSize);

        return new Position<int>(x, y, z);
    }
}