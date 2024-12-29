using System.Runtime.CompilerServices;
using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.World;

public class Chunk
{
    /// <summary>
    /// Creates a chunk at the given position and initializes a Memory buffer
    /// for voxel data. The buffer is initialized with transparent voxels.
    /// </summary>
    /// <param name="position">World position of the chunk.</param>
    /// <param name="chunkSize">Size of the chunk along one dimension (default: 16).</param>
    public Chunk(Position<int> position, int chunkSize = 16)
    {
        Position = position;

        ChunkSize = chunkSize;
        ChunkArea = ChunkSize * ChunkSize;
        ChunkVolume = ChunkSize * ChunkSize * ChunkSize;

        // Initialize our Memory<Voxel> buffer with the specified capacity
        VoxelBuffer = new Memory<Voxel>(new Voxel[ChunkVolume]);

        // Fill the Memory<Voxel> with transparent voxels
        var span = VoxelBuffer.Span;
            
        var transparentVoxel = new Voxel(Color.Transparent);
            
        for (var i = 0; i < span.Length; i++)
        {
            span[i] = transparentVoxel;
        }
    }

    /// <summary>
    /// Gets the size of the chunk along one dimension.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets the 2D area of the chunk (ChunkSize * ChunkSize).
    /// </summary>
    public int ChunkArea { get; }

    /// <summary>
    /// Gets the total volume of the chunk (ChunkSize^3).
    /// </summary>
    public int ChunkVolume { get; }

    /// <summary>
    /// Indicates whether the chunk requires remeshing or other updates.
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Holds the chunk's voxel data in a Memory buffer to minimize heap allocations.
    /// </summary>
    private Memory<Voxel> VoxelBuffer { get; }

    /// <summary>
    /// The world position of this chunk.
    /// </summary>
    public Position<int> Position { get; }

    /// <summary>
    /// Sets a voxel at the specified local chunk coordinates, marking the chunk dirty.
    /// </summary>
    /// <param name="position">Local position within the chunk.</param>
    /// <param name="value">Voxel data to set.</param>
    /// <returns>True if successful, otherwise false (not yet implemented).</returns>
    public void SetVoxel(Position<int> position, Voxel value)
    {
        var index = GetVoxelIndex(position);
        VoxelBuffer.Span[index] = value;
        IsDirty = true;
    }

    /// <summary>
    /// Gets a voxel at the specified local chunk coordinates.
    /// </summary>
    /// <param name="position">Local position within the chunk.</param>
    /// <returns>The voxel at the given position.</returns>
    public Voxel GetVoxel(Position<int> position)
    {
        var index = GetVoxelIndex(position);
        return VoxelBuffer.Span[index];
    }

    /// <summary>
    /// Calculates the flat index in VoxelBuffer for a 3D coordinate within this chunk.
    /// </summary>
    /// <param name="position">Local position within the chunk.</param>
    /// <returns>Zero-based index within the VoxelBuffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetVoxelIndex(Position<int> position)
    {
        return position.X
               + (ChunkSize * position.Z)
               + (ChunkArea * position.Y);
    }

    /// <summary>
    /// Provides a span of the chunk's voxel data, useful for operations on a sub-region.
    /// </summary>
    /// <param name="xStart">Starting X coordinate within the chunk.</param>
    /// <param name="yStart">Starting Y coordinate within the chunk.</param>
    /// <param name="zStart">Starting Z coordinate within the chunk.</param>
    /// <param name="width">Width of the sub-region.</param>
    /// <param name="height">Height of the sub-region.</param>
    /// <param name="depth">Depth of the sub-region.</param>
    /// <returns>A Span of the requested sub-region.</returns>
    public Span<Voxel> GetVoxelSpan(int xStart, int yStart, int zStart,
        int width, int height, int depth)
    {
        // Calculate the flat offset of the starting point
        var offset = GetVoxelIndex(new Position<int>(xStart, yStart, zStart));

        // Calculate total length needed
        var length = width * height * depth;

        // Return a slice of the Memory<Voxel>
        return VoxelBuffer.Slice(offset, length).Span;
    }
        
    /// <summary>
    /// Provides a span of the chunk's voxel data, useful for operations on the entire chunk.
    /// </summary>
    /// <returns>A </returns>
    public Span<Voxel> GetVoxelSpan()
    {
        return VoxelBuffer.Span;
    }
}