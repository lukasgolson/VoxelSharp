using VoxelSharp.Core.World;

namespace VoxelSharp.Core.Interfaces.WorldGen;

/// <summary>
/// This interface is used to define a world generator. A world generator is responsible for generating the world's terrain.
/// </summary>
public interface IWorldGenerator
{
    /// <summary>
    /// This method is called when a new chunk is created and needs to be generated.
    /// </summary>
    /// <param name="chunk">The newly created empty chunk.</param>
    /// <returns>Whether the chunk was succesfully generated or not.</returns>
    public bool GenerateChunk(Chunk chunk);
}