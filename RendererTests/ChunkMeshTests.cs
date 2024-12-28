using OpenTK.Mathematics;
using VoxelSharp.Renderer.Mesh.World;
using VoxelSharp.Structs;
using VoxelSharp.World;

namespace RendererTests;

public class ChunkMeshTests
{
    [Fact]
    public void GetModelMatrix_ValidInputs_ReturnsExpected_Identity()
    {
        // Arrange
        var chunk = new Chunk(new Position<int>(0, 0, 0), 16);
        var chunkMesh = new ChunkMesh(chunk);

        // Act
        var result = chunkMesh.GetModelMatrix();

        // Assert
        Assert.Equal(Matrix4.Identity, result);
    }

    [Fact]
    public void GetModelMatrix_NonZeroPosition_ReturnsTranslatedMatrix()
    {
        // Arrange
        var position = new Position<int>(10, 20, 30);
        var chunk = new Chunk(position, 16);
        var chunkMesh = new ChunkMesh(chunk);

        // Act
        var result = chunkMesh.GetModelMatrix();

        // Assert
        var expectedMatrix = Matrix4.CreateTranslation(10 * 16, 20 * 16, 30 * 16);
        Assert.Equal(expectedMatrix, result);

    }
}