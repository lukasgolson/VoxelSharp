using OpenTK.Mathematics;
using VoxelSharp.Renderer.Mesh.World;
using VoxelSharp.Structs;
using VoxelSharp.World;

namespace RendererTests;

public class ChunkTests
{
    [Fact]
    public void Constructor_ValidInputs_ReturnsExpected()
    {
        // Arrange
        var position = new Position<int>(0, 0, 0);
        var chunkSize = 16;
        
        // Act
        var chunk = new Chunk(position, chunkSize);
        
        // Assert
        Assert.Equal(position, chunk.ChunkCoordinates);
        Assert.Equal(chunkSize, chunk.ChunkSize);
        Assert.Equal(chunkSize * chunkSize, chunk.ChunkArea);
        Assert.Equal(chunkSize * chunkSize * chunkSize, chunk.ChunkVolume);
    }

    [Fact]
    public void Constructor_NegativeChunkSize_ThrowsArgumentException()
    {
        // Arrange
        var position = new Position<int>(0, 0, 0);
        int chunkSize = -1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Chunk(position, chunkSize));
    }

    [Fact]
    public void ChunkSize_Property_ReturnsCorrectValue()
    {
        // Arrange
        var chunk = new Chunk(new Position<int>(0, 0, 0), 32);

        // Act
        int result = chunk.ChunkSize;

        // Assert
        Assert.Equal(32, result);
    }

    [Fact]
    public void Position_Property_ReturnsCorrectValue()
    {
        // Arrange
        var position = new Position<int>(5, 10, 15);
        var chunk = new Chunk(position, 16);

        // Act
        var result = chunk.ChunkCoordinates;

        // Assert
        Assert.Equal(position, result);
    }
}