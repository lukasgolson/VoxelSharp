namespace RendererTests;

public class MathTests
{
    [Theory]
    [InlineData(10, 3, 1)] // Positive numbers
    [InlineData(-10, 3, 2)] // Negative a
    [InlineData(10, 1, 0)] // b = 1
    [InlineData(0, 3, 0)] // a = 0
    public void EuclideanModulo_ValidInputs_ReturnsExpected(int a, int b, int expected)
    {
        // Act
        int result = VoxelSharp.Math.EModulo(a, b);

        // Assert
        Assert.Equal(expected, result);
    }


    [Fact]
    public void EuclideanModulo_DivideByZero_ThrowsException()
    {
        Assert.Throws<DivideByZeroException>(() => VoxelSharp.Math.EModulo(10, 0));
    }

    [Fact]
    public void EuclideanModulo_ModuloNegative_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => VoxelSharp.Math.EModulo(10, -1));
    }
}