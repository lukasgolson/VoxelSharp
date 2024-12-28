namespace VoxelSharp.Core;

public static class Math
{
    /// <summary>
    ///     Computes the Euclidean modulo operation, ensuring a non-negative result.
    /// </summary>
    /// <param name="value">The value to be divided.</param>
    /// <param name="mod">The divisor.</param>
    /// <returns>The remainder of the division, adjusted to be non-negative.</returns>
    public static int EModulo(int value, int mod)
    {
        return (value % mod + mod) % mod;
    }
}